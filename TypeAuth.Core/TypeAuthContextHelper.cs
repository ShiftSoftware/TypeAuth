using ShiftSoftware.TypeAuth.Core.Actions;
using Action = ShiftSoftware.TypeAuth.Core.Actions.Action;

namespace ShiftSoftware.TypeAuth.Core
{
    internal class TypeAuthContextHelper
    {
        List<ActionBankItem> ActionBank { get; set; }

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
                    var value = (Action?)y.GetValue(y);

                    if (value is not null)
                        treeDictionary[y.Name] = value;
                });

                tree.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).Where(y => y.ReturnType == typeof(List<DynamicAction>)).ToList().ForEach(y =>
                {
                    var invoked = (y.Invoke(y, new object[] { this }) as List<DynamicAction>);

                    if (invoked != null)
                        treeDictionary[y.Name] = invoked.ToDictionary(z => z.Id.ToString(), z => (object)new DynamicAction(z.Id.ToString(), z.Name, z.Type));
                });
            }

            return rootDictionary;
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

            if (actionCursor.GetType() == typeof(Dictionary<string, object>))
            {
                var theDictionary = (Dictionary<string, object>)actionCursor;

                foreach (var key in theDictionary.Keys)
                {
                    //Wild Card: Access already provided at this Level of the Access Tree. But the action tree has more child nodes.
                    //The current Access is simply passed to every child node of the the current Action Node
                    if (accessTypes.Count > 0 || accessValue != null)
                    {
                        this.PopulateActionBank(theDictionary[key], accessCursor);
                    }
                    else
                    {
                        if (accessCursor != null)
                        {
                            var accessCursorDictionary = (Newtonsoft.Json.Linq.JObject)accessCursor;

                            PopulateActionBank(theDictionary[key], accessCursorDictionary[key]);
                        }
                    }

                }
            }

            if ((actionCursor.GetType() == typeof(Action) || actionCursor.GetType().BaseType == typeof(Action) || actionCursor.GetType() == typeof(DynamicAction)) && (accessTypes.Count > 0 || accessValue != null))
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

            var theDynamicAction = actionToCheck as DynamicAction;

            if (theDynamicAction is not null)
            {
                actionMatches = this.ActionBank.Where(x => x.Action.GetType() == typeof(DynamicAction)).Where(x => ((DynamicAction)x.Action).Id.Equals(theDynamicAction.Id)).ToList();
            }
            else
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
