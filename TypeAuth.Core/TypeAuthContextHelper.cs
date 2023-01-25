using Newtonsoft.Json.Linq;
using ShiftSoftware.TypeAuth.Core.Actions;
using System.Collections;
using System.Reflection;
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

        internal ActionTreeItem GenerateActionTree(List<Type> actionTrees, List<string> accessTreeJSONStrings, ActionTreeItem? rootActionTree = null, string? jsonPath = null)
        {
            if (rootActionTree is null)
                rootActionTree = new ActionTreeItem() { TypeName = "Root" };

            if (jsonPath is null)
                jsonPath = "";

            foreach (var tree in actionTrees)
            {
                var treeAttribute = tree.GetCustomAttribute((typeof(ActionTree))) as ActionTree;

                var actionTreeItem = new ActionTreeItem() { TypeName = tree.Name, Type = tree };

                if (treeAttribute != null)
                {
                    actionTreeItem.DisplayName = treeAttribute.Name;
                    actionTreeItem.DisplayDescription = treeAttribute.Description;
                }

                rootActionTree.ActionTreeItems.Add(actionTreeItem);
                
                jsonPath += tree.Name + ".";

                var childTress = tree.GetNestedTypes().ToList().Where(x => x.GetCustomAttributes(typeof(ActionTree), false) != null).ToList();

                GenerateActionTree(childTress, accessTreeJSONStrings, actionTreeItem, jsonPath);

                tree.GetFields(BindingFlags.Public | BindingFlags.Static).ToList().ForEach(y =>
                {
                    var value = y.GetValue(y);

                    if (value != null && (value as Action) != null)
                    {
                        var action = (Action)value;

                        actionTreeItem.ActionTreeItems.Add(new ActionTreeItem
                        {
                            TypeName = y.Name,
                            Action = action,
                            DisplayName = action.Name,
                            DisplayDescription = action.Description,
                            Type = y.FieldType
                        });
                    }

                    else if (value != null && (value as DynamicActionBase) != null)
                    {
                        var dynamicAction = (DynamicActionBase)value;

                        var fullName = jsonPath + y.Name;

                        var keys = new List<string>();

                        foreach (var key in fullName.Split('.'))
                        {
                            keys.Add(key);

                            var path = string.Join(".", keys);

                            foreach (var jsonString in accessTreeJSONStrings)
                            {
                                var accessTree = JObject.Parse(jsonString);

                                var access = accessTree.SelectToken(path);

                                if (access != null && access.GetType() == typeof(JArray))
                                {
                                    dynamicAction.UnderlyingAction = dynamicAction.GenerateAction();

                                    dynamicAction.Dictionary[$"{Guid.NewGuid()}-{Guid.NewGuid()}"] = dynamicAction.UnderlyingAction;
                                }
                            }
                        }

                        foreach (var jsonString in accessTreeJSONStrings)
                        {
                            var accessTree = JObject.Parse(jsonString);

                            var access = accessTree.SelectToken(fullName);

                            if (access != null && access.GetType() == typeof(JObject))
                            {
                                var jobject = (access as JObject)!;

                                foreach (var key in jobject)
                                {
                                    dynamicAction.Dictionary[key.Key] = dynamicAction.GenerateAction();
                                }
                            }
                        }

                        var dynamicRoot = new ActionTreeItem() { TypeName = y.Name };

                        actionTreeItem.ActionTreeItems.Add(dynamicRoot);

                        foreach (var item in dynamicAction.Dictionary)
                        {
                            dynamicRoot.ActionTreeItems.Add(new ActionTreeItem { TypeName = item.Key, Action = item.Value });
                        }
                    }

                });

                //Reset the json path after all recursive calls
                jsonPath = null;
            }

            return rootActionTree;
        }

        internal void PopulateActionBank(ActionTreeItem actionCursor, object? accessCursor)
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

            if (actionCursor.ActionTreeItems.Count > 0)
            {
                //var dictionaryEntries = CastDict((IDictionary)actionCursor); //(Dictionary<string, object>)actionCursor;

                foreach (var entry in actionCursor.ActionTreeItems)
                {
                    //Wild Card: Access already provided at this Level of the Access Tree. But the action tree has more child nodes.
                    //The current Access is simply passed to every child node of the the current Action Node
                    if (accessTypes.Count > 0 || accessValue != null)
                    {
                        actionCursor.WildCardAccess = accessTypes;
                        this.PopulateActionBank(entry, accessCursor);
                    }
                    else
                    {
                        if (accessCursor != null)
                        {
                            var accessCursorDictionary = (JObject) accessCursor;

                            PopulateActionBank(entry, accessCursorDictionary[entry.TypeName]);
                        }
                    }

                }
            }

            if (actionCursor.Action != null && (accessTypes.Count > 0 || accessValue != null))
            {
                var theAction = (Action) actionCursor.Action;

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

        internal bool Can(Action actionToCheck, Access accessTypeToCheck)
        {
            var access = false;

            var actionMatches = this.LocateActionInBank(actionToCheck);

            access = actionMatches.Any(actionMatch => actionMatch.AccessList.IndexOf(accessTypeToCheck) > -1);

            return access;
        }
    }
}
