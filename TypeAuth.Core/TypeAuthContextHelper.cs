﻿using ShiftSoftware.TypeAuth.Core.Actions;

using System.Reflection;

namespace ShiftSoftware.TypeAuth.Core
{
    internal class TypeAuthContextHelper
    {
        internal List<ActionBankItem> ActionBank { get; set; }

        public TypeAuthContextHelper()
        {
            ActionBank = new List<ActionBankItem>();
        }

        internal ActionTreeItem GenerateActionTree(List<Type> actionTrees, List<string> accessTreeJSONStrings, ActionTreeItem? rootActionTree = null)
        {
            if (rootActionTree is null)
                rootActionTree = new ActionTreeItem() { TypeName = "Root" };

            foreach (var tree in actionTrees)
            {
                var treeAttribute = tree.GetCustomAttribute((typeof(ActionTree))) as ActionTree;

                var actionTreeItem = new ActionTreeItem() { TypeName = tree.Name, Type = tree };

                if (treeAttribute != null)
                {
                    //actionTreeItem.ID = treeAttribute.ID;
                    actionTreeItem.DisplayName = treeAttribute.Name;
                    actionTreeItem.DisplayDescription = treeAttribute.Description;
                }

                rootActionTree.ActionTreeItems.Add(actionTreeItem);
                
                var childTress = tree.GetNestedTypes().ToList().Where(x => x.GetCustomAttributes(typeof(ActionTree), false) != null).ToList();

                GenerateActionTree(childTress, accessTreeJSONStrings, actionTreeItem);

                tree.GetFields(BindingFlags.Public | BindingFlags.Static).ToList().ForEach(y =>
                {
                    var value = y.GetValue(y);

                    if (value != null && (value as ActionBase) != null)
                    {
                        var action = (ActionBase)value;

                        var thisActionTreeItem = new ActionTreeItem
                        {
                            TypeName = y.Name,
                            Action = action,
                            DisplayName = action.Name,
                            DisplayDescription = action.Description,
                            Type = y.FieldType
                        };

                        actionTreeItem.ActionTreeItems.Add(thisActionTreeItem);
                    }
                });
            }

            return rootActionTree;
        }

        internal void PopulateActionBank(ActionTreeItem actionCursor, object? accessCursor)
        {
            if (actionCursor.DynamicSubitem)
                return;

            AccessTreeNode node = new AccessTreeNode(accessCursor);

            if (actionCursor.ActionTreeItems.Count > 0)
            {
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

            if (actionCursor.Action != null && (node.AccessArray.Count > 0 || node.AccessValue != null || (actionCursor.Action is DynamicAction && node.AccessObject != null)))
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

                if (theAction  is DynamicAction && node.AccessArray.Count > 0)
                    actionCursor.WildCardAccess = node.AccessArray;

                this.ActionBank.Add(new ActionBankItem(theAction, node.AccessArray, node.AccessValue, node.AccessObject));
            }
        }

        internal void ExpandDynamicActions(ActionTreeItem actionTreeItem)
        {
            foreach (var item in actionTreeItem.ActionTreeItems)
            {
                ExpandDynamicActions(item);
            }

            var action = actionTreeItem.Action;

            if (action == null)
                return;

            if (action is DynamicAction)
            {
                var dynamicAction = action as DynamicAction;

                foreach (var item in dynamicAction!.Items)
                {
                    var newTreeItem = new ActionTreeItem
                    {
                        Action = action,
                        DisplayName = item.Value,
                        TypeName = item.Key,
                        Type = action.GetType(),
                        WildCardAccess = new List<Access>(),
                        DynamicSubitem = true
                    };

                    actionTreeItem.ActionTreeItems.Add(newTreeItem);
                }
            }
        }

        internal List<ActionBankItem> LocateActionInBank(ActionBase actionToCheck, string? Id = null, string? selfId = null)
        {
            List<ActionBankItem> actionMatches = new List<ActionBankItem> { };

            foreach (var item in this.ActionBank.Where(x => x.Action == actionToCheck).ToList())
            {
                    actionMatches.Add(item);

                if (actionToCheck is DynamicAction)
                {
                    actionMatches.AddRange(item.SubActionBankItems.Where(x =>
                    {
                        var action = (x.Action as DynamicAction)!;

                        return action.Id == Id || (Id != null && selfId != null && Id == selfId && action.Id == TypeAuthContext.SelfRererenceKey);
                    }));
                }
            }

            //Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(actionMatches, Newtonsoft.Json.Formatting.Indented));

            return actionMatches
                .Where(x =>
                {
                    var dynamicAction = x.Action as DynamicAction;

                    if (Id != null && dynamicAction != null && dynamicAction.Id == null && x.AccessList.Count == 0)
                        return false;

                    return true;
                }).ToList();
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
