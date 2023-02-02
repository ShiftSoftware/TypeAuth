using Newtonsoft.Json.Linq;
using ShiftSoftware.TypeAuth.Core.Actions;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
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

                    if (value != null && (value as ActionBase) != null)
                    {
                        var action = (ActionBase)value;

                        actionTreeItem.ActionTreeItems.Add(new ActionTreeItem
                        {
                            TypeName = y.Name,
                            Action = action,
                            DisplayName = action.Name,
                            DisplayDescription = action.Description,
                            Type = y.FieldType
                        });
                    }

                    //else if (value != null && (value as DynamicActionBase) != null)
                    //{
                    //    var dynamicAction = (DynamicActionBase)value;

                    //    var fullName = jsonPath + y.Name;

                    //    var keys = new List<string>();

                    //    foreach (var key in fullName.Split('.'))
                    //    {
                    //        keys.Add(key);

                    //        var path = string.Join(".", keys);

                    //        foreach (var jsonString in accessTreeJSONStrings)
                    //        {
                    //            var accessTree = JObject.Parse(jsonString);

                    //            var access = accessTree.SelectToken(path);

                    //            if (access != null && access.GetType() == typeof(JArray))
                    //            {
                    //                var id = $"{Guid.NewGuid()}-{Guid.NewGuid()}";

                    //                dynamicAction.UnderlyingAction = dynamicAction.GenerateAction(dynamicAction.GetHashCode() + "." + fullName + "." + id);

                    //                dynamicAction.Dictionary[id] = dynamicAction.UnderlyingAction;
                    //            }
                    //        }
                    //    }

                    //    foreach (var jsonString in accessTreeJSONStrings)
                    //    {
                    //        var accessTree = JObject.Parse(jsonString);

                    //        var access = accessTree.SelectToken(fullName);

                    //        if (access != null && access.GetType() == typeof(JObject))
                    //        {
                    //            var jobject = (access as JObject)!;

                    //            foreach (var key in jobject)
                    //            {
                    //                dynamicAction.Dictionary[key.Key] = dynamicAction.GenerateAction(dynamicAction.GetHashCode() + "." + fullName + "." + key.Key);
                    //            }
                    //        }
                    //    }

                    //    var dynamicRoot = new ActionTreeItem()
                    //    {
                    //        TypeName = y.Name,
                    //        Action = dynamicAction.UnderlyingAction,
                    //        DisplayName = dynamicAction.UnderlyingAction?.Name,
                    //        DisplayDescription = dynamicAction.UnderlyingAction?.Description,
                    //        DynamicAction = dynamicAction,
                    //        Type = dynamicAction.GetType(),
                    //    };

                    //    actionTreeItem.ActionTreeItems.Add(dynamicRoot);

                    //    foreach (var item in dynamicAction.Dictionary)
                    //    {
                    //        dynamicRoot.ActionTreeItems.Add(new ActionTreeItem
                    //        {
                    //            TypeName = item.Key,
                    //            Action = item.Value,
                    //            DisplayName = item.Value.Name,
                    //            DisplayDescription = item.Value.Description,
                    //            DynamicAction = dynamicAction,
                    //            Type = item.Value.GetType()
                    //        });
                    //    }
                    //}

                });

                //Reset the json path after all recursive calls
                jsonPath = null;
            }

            return rootActionTree;
        }

        internal void PopulateActionBank(ActionTreeItem actionCursor, object? accessCursor)
        {
            AccessTreeNode node = new AccessTreeNode(accessCursor);

            if (actionCursor.ActionTreeItems.Count > 0)
            {
                //var dictionaryEntries = CastDict((IDictionary)actionCursor); //(Dictionary<string, object>)actionCursor;

                foreach (var entry in actionCursor.ActionTreeItems)
                {
                    //Wild Card: Access already provided at this Level of the Access Tree. But the action tree has more child nodes.
                    //The current Access is simply passed to every child node of the the current Action Node
                    if (node.AccessArray.Count > 0 || node.AccessValue != null)
                    {
                        actionCursor.WildCardAccess = node.AccessArray;
                        this.PopulateActionBank(entry, accessCursor);
                    }
                    else
                    {
                        if (accessCursor != null)
                            PopulateActionBank(entry, node.AccessObject![entry.TypeName]);
                    }
                }
            }

            if (actionCursor.Action != null && (node.AccessArray.Count > 0 || node.AccessValue != null || (actionCursor.Action.GetType().BaseType == typeof(DynamicAction) &&  node.AccessObject != null)))
            {
                var theAction = (ActionBase)actionCursor.Action;

                if (theAction.Type == ActionType.Text && node.AccessValue == null)
                {
                    var textAction = theAction as TextAction;
                    var dynamicTextAction = theAction as DynamicTextAction;

                    var maximumAccess = textAction?.MaximumAccess ?? dynamicTextAction?.MaximumAccess;
                    var minimumAccess = textAction?.MinimumAccess ?? dynamicTextAction?.MinimumAccess;

                    if (node.AccessArray.Contains(Access.Maximum))
                        node.AccessValue = maximumAccess;
                    else
                        node.AccessValue = minimumAccess;
                }

                this.ActionBank.Add(new ActionBankItem(theAction, node.AccessArray, node.AccessValue, node.AccessObject));
            }
        }

        internal List<ActionBankItem> LocateActionInBank(ActionBase actionToCheck, string? Id = null, string? selfId = null)
        {
            List<ActionBankItem> actionMatches = new List<ActionBankItem> { };

            foreach (var item in this.ActionBank.Where(x => x.Action == actionToCheck).ToList())
            {
                actionMatches.Add(item);

                if (actionToCheck.GetType().BaseType == typeof(DynamicAction))
                {
                    actionMatches.AddRange(item.SubActionBankItems.Where(x =>
                    {
                        var action = (x.Action as DynamicAction)!;

                        return action.Id == Id || (Id != null && selfId != null && Id == selfId && action.Id == TypeAuthContext.SelfRererenceKey);
                    }));
                }
            }

            //Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(actionMatches, Newtonsoft.Json.Formatting.Indented));

            return actionMatches;
        }

        internal bool Can(ActionBase actionToCheck, Access accessTypeToCheck, string? Id = null, string? selfId = null)
        {
            var access = false;

            var actionMatches = this.LocateActionInBank(actionToCheck, Id, selfId);

            access = actionMatches.Any(actionMatch => actionMatch.AccessList.IndexOf(accessTypeToCheck) > -1);

            return access;
        }
    }
}
