using ShiftSoftware.TypeAuth.Core.Actions;

namespace ShiftSoftware.TypeAuth.Core;

public static class AccessTreeGenerator
{
    public static void SetAccessValue(this TypeAuthContext typeAuthContext, TextAction theAction, string? value, string? maximumValue)
    {
        typeAuthContext.SetTextAccessValue(theAction, theAction.MinimumAccess, theAction.MaximumAccess, value, maximumValue, theAction.Comparer);
    }

    public static void SetAccessValue(this TypeAuthContext typeAuthContext, DynamicTextAction theAction, string? Id, string? value, string? maximumValue)
    {
        typeAuthContext.SetTextAccessValue(theAction, theAction.MinimumAccess, theAction.MaximumAccess, value, maximumValue, theAction.Comparer, Id);
    }

    private static void SetTextAccessValue(this TypeAuthContext typeAuthContext, ActionBase theAction, string? actionMin, string? actionMax, string? valueToSet, string? maxAllowed, Func<string?, string?, string?>? comparer, string? Id = null)
    {

        var actionMatches = typeAuthContext.FindOrAddInActionBank(theAction, Id);

        foreach (var action in actionMatches)
        {
            valueToSet = typeAuthContext.ReduceValue(comparer, actionMin, actionMax, valueToSet, maxAllowed);

            action.AccessValue = valueToSet;
        }
    }

    private static string? ReduceValue(this TypeAuthContext typeAuthContext, Func<string?, string?, string?>? comparer, string? actionMinimum, string? actionMaximum, string? value, string? maximumValue)
    {
        if (comparer != null)
        {
            //Only used to parse the value.
            //For example if the comparer deals with numbers. And a number like 000100 is provided. Comparing the value against it self will return 100
            value = comparer(value, value);

            var assignableMaximumWinner = comparer(maximumValue, actionMaximum);

            if (assignableMaximumWinner == actionMaximum)
                assignableMaximumWinner = maximumValue;

            var actionMaximumWinner = comparer(value, assignableMaximumWinner);

            if (actionMaximumWinner == value)
                value = assignableMaximumWinner;

            var minimumWinner = comparer(value, actionMinimum);

            if (minimumWinner == actionMinimum)
                value = actionMinimum;
        }

        return value;
    }

    public static void ToggleAccess(this TypeAuthContext typeAuthContext, ActionBase theAction, Access access, string? Id = null)
    {
        var actionMatches = typeAuthContext.FindOrAddInActionBank(theAction, Id);

        foreach (var action in actionMatches)
        {
            if (action.AccessList.Contains(access))
                action.AccessList.Remove(access);
            else
                action.AccessList.Add(access);
        }
    }

    private static List<ActionBankItem> FindOrAddInActionBank(this TypeAuthContext typeAuthContext, ActionBase theAction, string? Id)
    {
        var actionMatches = typeAuthContext.TypeAuthContextHelper.LocateActionInBank(theAction, Id);

        if (actionMatches.Count == 0 && theAction is Actions.Action)
        {
            var actionBankItem = new ActionBankItem(theAction, new List<Access>());

            typeAuthContext.TypeAuthContextHelper.ActionBank.Add(actionBankItem);

            actionMatches.Add(actionBankItem);
        }

        if (theAction is DynamicAction)
        {
            if (actionMatches.Count == 0)
            {
                var actionBankItem = new ActionBankItem(theAction, new List<Access>());

                actionBankItem.SubActionBankItems.Add(new ActionBankItem(new DynamicAction { Id = Id }, new List<Access> { }));

                typeAuthContext.TypeAuthContextHelper.ActionBank.Add(actionBankItem);

                actionMatches = typeAuthContext.TypeAuthContextHelper.LocateActionInBank(theAction, Id);
            }


            if (!actionMatches.Any(x => (x.Action as DynamicAction)!.Id == Id))
            {
                actionMatches.First().SubActionBankItems.Add(new ActionBankItem(new DynamicAction { Id = Id, Type = theAction.Type }, new List<Access>()));

                actionMatches = typeAuthContext.TypeAuthContextHelper.LocateActionInBank(theAction, Id);
            }
        }

        return actionMatches;
    }

    public static string GenerateAccessTree(this TypeAuthContext typeAuthContext, TypeAuthContext reducer, TypeAuthContext? preserver = null)
    {
        var reducedActionTreeItems = new List<ActionTreeNode>();

        typeAuthContext.FlattenActionTree(reducedActionTreeItems, reducer.ActionTree);

        List<ActionTreeNode>? preservedActionTreeItems = null;

        if (preserver != null)
        {
            preservedActionTreeItems = new List<ActionTreeNode>();
            typeAuthContext.FlattenActionTree(preservedActionTreeItems, preserver.ActionTree);
        }

        var accessTree = typeAuthContext.TraverseActionTree(typeAuthContext.ActionTree, new Dictionary<string, object>(), reducer, reducedActionTreeItems, false, preserver, preservedActionTreeItems);

        if (accessTree == null)
            accessTree = new Dictionary<string, object>(); //To return an empty json {}

        return Newtonsoft.Json.JsonConvert.SerializeObject(accessTree);
    }

    private static void FlattenActionTree(this TypeAuthContext typeAuthContext, List<ActionTreeNode> flattenedActionTreeItems, ActionTreeNode root)
    {
        foreach (var item in root.ActionTreeItems)
        {
            typeAuthContext.FlattenActionTree(flattenedActionTreeItems, item);
        }

        if (!root.IsADynamicSubItem)
            flattenedActionTreeItems.Add(root);
    }

    private static object? TraverseActionTree(this TypeAuthContext typeAuthContext, ActionTreeNode actionTreeItem, Dictionary<string, object> accessTree, TypeAuthContext reducer, List<ActionTreeNode> reducedActionTreeItems, bool stopTraversing = false, TypeAuthContext? preserver = null, List<ActionTreeNode>? preservedActionTreeItems = null)
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

            var access = actionTreeItem.WildCardAccess.Where(x => reducerActionTreeItem.WildCardAccess.Contains(x)).ToList();

            if (preserverActionTreeItem != null)
            {
                foreach (var item in preserverActionTreeItem.WildCardAccess)
                {
                    if (!reducerActionTreeItem.WildCardAccess.Contains(item) && !access.Contains(item))
                    {
                        access.Add(item);
                    }
                }
            }

            if (access.Count() > 0)
                //return null;

                return access;
        }

        foreach (var subActionTreeItem in actionTreeItem.ActionTreeItems)
        {
            var value = typeAuthContext.TraverseActionTree(subActionTreeItem, new Dictionary<string, object>(), reducer, reducedActionTreeItems, stopTraversing, preserver, preservedActionTreeItems);

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

                        var value = typeAuthContext.TraverseActionTree(subActionTreeItem, dynamicItems, reducer, reducedActionTreeItems, true, preserver, preservedActionTreeItems);

                        if (value != null)
                            dynamicItems![subActionTreeItem.ID] = value;
                    }

                    if (dynamicItems.Count == 0)
                        return null;

                    return dynamicItems;
                }
            }

            if (actionTreeItem.Action.Type == ActionType.Text)
            {
                var textAction = actionTreeItem.Action as TextAction;
                var dynamicTextAction = actionTreeItem.Action as DynamicTextAction;

                string? value = null;

                if (dynamicTextAction != null)
                    value = typeAuthContext.AccessValue(dynamicTextAction, actionTreeItem.ID);
                else if (textAction != null)
                    value = typeAuthContext.AccessValue(textAction);

                string? reducedValue = null;

                if (dynamicTextAction != null)
                    reducedValue = reducer.AccessValue(dynamicTextAction, actionTreeItem.ID);
                else if (textAction != null)
                    reducedValue = reducer.AccessValue(textAction);

                if (dynamicTextAction != null)
                    value = typeAuthContext.ReduceValue(dynamicTextAction.Comparer, dynamicTextAction.MinimumAccess, dynamicTextAction.MaximumAccess, value, reducedValue);
                else if (textAction != null)
                    value = typeAuthContext.ReduceValue(textAction.Comparer, textAction.MinimumAccess, textAction.MaximumAccess, value, reducedValue);


                if (value == null || value == textAction?.MinimumAccess || value == dynamicTextAction?.MinimumAccess)
                    return null;

                if (textAction is DecimalAction || dynamicTextAction is DynamicDecimalAction)
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

    public static Dictionary<ActionBase, string> FindInAccessibleActionsOn(this TypeAuthContext typeAuthContext, TypeAuthContext typeAuthContextToCompare)
    {
        var inAccessibleActions = new Dictionary<ActionBase, string>();

        foreach (var actionBankItem in typeAuthContextToCompare.TypeAuthContextHelper.ActionBank)
        {
            var action = actionBankItem.Action;

            if (action is not null)
            {
                var inAccessibleForThisAction = new List<object>();

                foreach (var access in actionBankItem.AccessList)
                {
                    if (!typeAuthContext.Can(action, access))
                    {
                        if (action is BooleanAction && access != Access.Maximum)
                            continue;

                        if (action is ReadAction && access != Access.Read)
                            continue;

                        if (action is ReadWriteAction && !(access == Access.Read || access == Access.Write))
                            continue;

                        if (action is ReadWriteDeleteAction && !(access == Access.Read || access == Access.Write || access == Access.Delete))
                            continue;

                        inAccessibleForThisAction.Add(access);
                    }
                }

                if (inAccessibleForThisAction.Count > 0)
                    inAccessibleActions[action] = string.Join(", ", inAccessibleForThisAction);

                if (action is DecimalAction decimalAction)
                {
                    var targetValue = typeAuthContextToCompare.AccessValue(decimalAction);
                    var allowedValue = typeAuthContext.AccessValue(decimalAction);


                    if (targetValue > allowedValue)
                    {
                        inAccessibleActions[action] = $"Maximum allowed value is {allowedValue}. You can't grant {targetValue}";
                    }
                }
            }
        }

        return inAccessibleActions;
    }
}