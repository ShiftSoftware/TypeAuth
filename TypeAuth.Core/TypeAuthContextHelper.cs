using ShiftSoftware.TypeAuth.Core.Actions;
using System.Collections;
using Action = ShiftSoftware.TypeAuth.Core.Actions.Action;

namespace ShiftSoftware.TypeAuth.Core
{
    internal class TypeAuthContextHelper
    {
        internal List<ActionBankItem> ActionBank { get; set; }

        public TypeAuthContextHelper()
        {
            ActionBank = new List<ActionBankItem>();
        }

        internal Dictionary<string, object> GenerateActionTree(List<Type> actionTrees, Dictionary<string, object>? rootDictionary = null)
        {
            if (rootDictionary is null)
                rootDictionary = new Dictionary<string, object>();

            foreach (var tree in actionTrees)
            {
                var treeDictionary = new Dictionary<string, object>();

                rootDictionary[tree.Name] = treeDictionary;

                var childTress = tree.GetNestedTypes().ToList().Where(x => x.GetCustomAttributes(typeof(ActionTree), false) != null).ToList();

                GenerateActionTree(childTress, treeDictionary);

                tree.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).ToList().ForEach(y =>
                {
                    var value = y.GetValue(y);

                    if (value != null && (value as Action) != null)
                        treeDictionary[y.Name] = (Action)value;

                    else if (value != null && value.GetType().GetGenericTypeDefinition() == typeof(DynamicAction<>))
                    {
                        var dict = value as DynamicActionDictionaryBase;

                        treeDictionary[y.Name] = dict!.Dictionary;
                    }
                });

                //tree.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).Where(y => y.ReturnType == typeof(DynamicActionList<Action>)).ToList().ForEach(y =>
                //{
                //    var invoked = (y.Invoke(y, new object[] { }) as DynamicActionList<Action>);

                //    if (invoked != null)
                //        treeDictionary[y.Name] = invoked.ToDictionary(z => z.Id?.ToString(), z => z);
                //});
            }

            return rootDictionary;
        }

        private IEnumerable<DictionaryEntry> CastDict(IDictionary dictionary)
        {
            foreach (DictionaryEntry entry in dictionary)
            {
                yield return entry;
            }
        }

        internal void PopulateActionBank(object actionCursor, object? accessCursor)
        {
            var accessTypes = new List<Access>();
            string? accessValue = null;

            if (accessCursor != null)
            {
                if (accessCursor.GetType() == typeof(Newtonsoft.Json.Linq.JValue))
                {
                    accessValue = accessCursor.ToString();
                }
                else if (accessCursor.GetType() == typeof(Newtonsoft.Json.Linq.JArray))
                {
                    var theArray = ((Newtonsoft.Json.Linq.JArray)accessCursor).Select(x => x.ToObject<Access>()).ToList();

                    accessTypes.AddRange(theArray);
                }
            }

            if (actionCursor.GetType().IsGenericType && actionCursor.GetType().GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                var dictionaryEntries = CastDict((IDictionary)actionCursor); //(Dictionary<string, object>)actionCursor;

                foreach (var entry in dictionaryEntries)
                {
                    //Wild Card: Access already provided at this Level of the Access Tree. But the action tree has more child nodes.
                    //The current Access is simply passed to every child node of the the current Action Node
                    if (accessTypes.Count > 0 || accessValue != null)
                    {
                        this.PopulateActionBank(entry.Value, accessCursor);
                    }
                    else
                    {
                        if (accessCursor != null)
                        {
                            var accessCursorDictionary = (Newtonsoft.Json.Linq.JObject)accessCursor;

                            PopulateActionBank(entry.Value, accessCursorDictionary[entry.Key]);
                        }
                    }

                }
            }

            if ((actionCursor.GetType() == typeof(Action) || actionCursor.GetType().BaseType == typeof(Action)) && (accessTypes.Count > 0 || accessValue != null))
            {
                var theAction = (Action)actionCursor;

                if (theAction.Type == ActionType.Text && accessValue == null)
                {
                    var textAction = (TextAction)theAction;
                    if (accessTypes.Contains(Access.Maximum))
                        accessValue = textAction.MaximumAccess;
                    else
                        accessValue = textAction.MinimumAccess;
                }

                this.ActionBank.Add(new ActionBankItem(theAction, accessTypes, accessValue));
            }
        }

        internal List<ActionBankItem> LocateActionInBank(Action actionToCheck)
        {
            List<ActionBankItem> actionMatches = new List<ActionBankItem> { };

            actionMatches = this.ActionBank.Where(x => x.Action == actionToCheck).ToList();

            //Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(actionMatches, Newtonsoft.Json.Formatting.Indented));

            return actionMatches;
        }

        internal bool CheckActionBank(Action actionToCheck, Access accessTypeToCheck)
        {
            var access = false;

            var actionMatches = this.LocateActionInBank(actionToCheck);

            access = actionMatches.Any(actionMatch => actionMatch.AccessList.IndexOf(accessTypeToCheck) > -1);

            return access;
        }
    }
}
