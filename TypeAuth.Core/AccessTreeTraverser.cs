using ShiftSoftware.TypeAuth.Core.Actions;

namespace ShiftSoftware.TypeAuth.Core;

internal static class AccessTreeTraverser
{
    internal static string? ReduceValue(ITextAccessProperties textProps, string? value, string? maximumValue)
    {
        var comparer = textProps.Comparer;

        if (comparer != null)
        {
            //Only used to parse the value.
            //For example if the comparer deals with numbers. And a number like 000100 is provided. Comparing the value against it self will return 100
            value = comparer(value, value);

            var assignableMaximumWinner = comparer(maximumValue, textProps.MaximumAccess);

            if (assignableMaximumWinner == textProps.MaximumAccess)
                assignableMaximumWinner = maximumValue;
            else
                assignableMaximumWinner = textProps.MaximumAccess;

            var actionMaximumWinner = comparer(value, assignableMaximumWinner);

            if (actionMaximumWinner == value)
                value = assignableMaximumWinner;

            var minimumWinner = comparer(value, textProps.MinimumAccess);

            if (minimumWinner == textProps.MinimumAccess)
                value = textProps.MinimumAccess;
        }

        return value;
    }

    internal static void FlattenActionTree(List<ActionTreeNode> flattenedActionTreeItems, ActionTreeNode root)
    {
        foreach (var item in root.ActionTreeItems)
        {
            FlattenActionTree(flattenedActionTreeItems, item);
        }

        if (!root.IsADynamicSubItem)
            flattenedActionTreeItems.Add(root);
    }

    internal static object? TraverseActionTree(TypeAuthContext typeAuthContext, ActionTreeNode actionTreeItem, Dictionary<string, object> accessTree, TypeAuthContext reducer, List<ActionTreeNode> reducedActionTreeItems, bool stopTraversing = false, TypeAuthContext? preserver = null, List<ActionTreeNode>? preservedActionTreeItems = null)
    {
        ActionTreeNode? preserverActionTreeItem = null;

        if (preservedActionTreeItems != null)
        {
            preserverActionTreeItem = preservedActionTreeItems.FirstOrDefault(
                x => x.Path == actionTreeItem.Path
            );
        }

        if (actionTreeItem.WildCardAccess.Count > 0 || preserverActionTreeItem?.WildCardAccess?.Count > 0)
        {
            var reducerActionTreeItem = reducedActionTreeItems.FirstOrDefault(
                x => x.Path == actionTreeItem.Path
            );

            var reducerWildCard = reducerActionTreeItem?.WildCardAccess ?? new List<Access>();

            var access = actionTreeItem.WildCardAccess.Where(x => reducerWildCard.Contains(x)).ToList();

            if (preserverActionTreeItem != null)
            {
                foreach (var item in preserverActionTreeItem.WildCardAccess)
                {
                    if (!reducerWildCard.Contains(item) && !access.Contains(item))
                    {
                        access.Add(item);
                    }
                }
            }

            if (access.Count() > 0)
                return access;
        }

        foreach (var subActionTreeItem in actionTreeItem.ActionTreeItems)
        {
            var value = TraverseActionTree(typeAuthContext, subActionTreeItem, new Dictionary<string, object>(), reducer, reducedActionTreeItems, stopTraversing, preserver, preservedActionTreeItems);

            if (value != null)
                accessTree[subActionTreeItem.ID] = value;
        }

        if (actionTreeItem.Action != null)
        {
            var dynamicAction = actionTreeItem.Action as DynamicAction;

            if (dynamicAction != null && !stopTraversing)
            {
                var actionBankItems = typeAuthContext.TypeAuthContextHelper.LocateActionInBank(dynamicAction);

                var subItems = actionBankItems.SelectMany(x => x.SubActionBankItems.ToList()).ToList();

                if (preserver != null)
                {
                    var preserverActionBankItems = preserver.TypeAuthContextHelper.LocateActionInBank(dynamicAction);

                    subItems.AddRange(preserverActionBankItems.SelectMany(x => x.SubActionBankItems.ToList()));

                    subItems = subItems.Distinct().ToList();
                }

                if (subItems.Count() > 0)
                {
                    var dynamicItems = new Dictionary<string, object>();

                    accessTree[actionTreeItem.ID] = dynamicItems;

                    foreach (var item in subItems)
                    {
                        var subActionTreeItem = new ActionTreeNode(actionTreeItem.Path)
                        {
                            Action = actionTreeItem.Action,
                            ID = (item.Action as DynamicAction)!.Id!
                        };

                        var value = TraverseActionTree(typeAuthContext, subActionTreeItem, dynamicItems, reducer, reducedActionTreeItems, true, preserver, preservedActionTreeItems);

                        if (value != null)
                            dynamicItems![subActionTreeItem.ID] = value;
                    }

                    if (dynamicItems.Count == 0)
                        return null;

                    return dynamicItems;
                }
            }

            if (actionTreeItem.Action.Type == ActionType.Text && actionTreeItem.Action is ITextAccessProperties textProps)
            {
                var isDynamic = actionTreeItem.Action is DynamicAction;
                var id = isDynamic ? actionTreeItem.ID : null;

                string? value = typeAuthContext.GetTextAccessValue(actionTreeItem.Action, textProps, id);
                string? reducedValue = reducer.GetTextAccessValue(actionTreeItem.Action, textProps, id);

                value = ReduceValue(textProps, value, reducedValue);

                if (value == null || value == textProps.MinimumAccess)
                    return null;

                if (actionTreeItem.Action is DecimalAction || actionTreeItem.Action is DynamicDecimalAction)
                    return decimal.Parse(value);

                return value;
            }

            else
            {
                var accesses = new List<Access>();

                foreach (var access in Enum.GetValues(typeof(Access)).Cast<Access>())
                {
                    if (dynamicAction != null)
                    {
                        if (typeAuthContext.Can(dynamicAction, access, actionTreeItem.ID) && reducer.Can(dynamicAction, access, actionTreeItem.ID))
                            accesses.Add(access);

                        if (preserver != null)
                        {
                            //If the access is reduced.
                            if (!reducer.Can(dynamicAction, access, actionTreeItem.ID))
                            {
                                //If the access should've been preserved.
                                if (preserver.Can(dynamicAction, access, actionTreeItem.ID) && !accesses.Contains(access))
                                {
                                    accesses.Add(access);
                                }
                            }
                        }
                    }
                    else if (actionTreeItem.Action is Actions.Action)
                    {
                        var action = (actionTreeItem.Action as Actions.Action)!;

                        if (typeAuthContext.TypeAuthContextHelper.Can(action, access) && reducer.TypeAuthContextHelper.Can(action, access))
                            accesses.Add(access);

                        if (preserver != null)
                        {
                            //If the access is reduced.
                            if (!reducer.Can(action, access))
                            {
                                //If the access should've been preserved.
                                if (preserver.Can(action, access) && !accesses.Contains(access))
                                {
                                    accesses.Add(access);
                                }
                            }
                        }
                    }
                }

                if (accesses.Count > 0)
                    return accesses;
                else
                    return null;
            }
        }

        if (accessTree.Count() == 0)
            return null;

        return accessTree;
    }
}
