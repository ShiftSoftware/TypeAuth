using System.Globalization;
using ShiftSoftware.TypeAuth.Core.Actions;

namespace ShiftSoftware.TypeAuth.Core;

internal static class AccessTreeTraverser
{
    internal static string? ReduceValue(ITextAccessProperties textProps, string? value, string? maximumValue)
    {
        return textProps.Combine.Reduce(value, maximumValue, textProps.MinimumAccess, textProps.MaximumAccess);
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

        List<Access>? wildCardAccess = null;

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

            //In the order of Access, not of the trees that granted it.
            access = Enum.GetValues(typeof(Access)).Cast<Access>().Where(access.Contains).ToList();

            //A text action's wildcard without Maximum gives its minimum, which is never more than its rows. When the trees
            //also list rows (one tree's wildcard on a group, another's rows), the rows are written.
            if (access.Count() > 0 && actionTreeItem.Action is DynamicAction rowsAction && rowsAction.Type == ActionType.Text && !access.Contains(Access.Maximum) &&
                typeAuthContext.TypeAuthContextHelper.LocateActionInBank(rowsAction).Any(x => x.SubActionBankItems.Count > 0))
                access.Clear();

            //Decided here: on a dynamic action, and on a group unless the context is written out for itself.
            var decidedHere = actionTreeItem.Action != null || WildCardWins(typeAuthContext, reducer, preserver);

            //A save never removes what the saver couldn't give back. Where writing this node's wildcard, or nothing for a
            //group wildcard the reducer doesn't hold, would remove stored grants at or beneath the node that the reducer
            //can't grant, the node is left as stored. The JSON can't hold a wildcard next to grants beneath it, and writing
            //them out one by one would not cover the actions and rows added later.
            if (decidedHere && !stopTraversing && preserver != null && preserverActionTreeItem != null && (access.Count > 0 || actionTreeItem.Action == null) &&
                StoredGoesBeyond(preserver, reducer, reducedActionTreeItems, preserverActionTreeItem, access))
                return TraverseActionTree(preserver, preserverActionTreeItem, new Dictionary<string, object>(), preserver, preservedActionTreeItems!);

            if (access.Count() > 0)
            {
                //The wildcard is written. A dynamic action is written as a wildcard or as rows, never both, so rows that go
                //beyond its wildcard cannot be written; the wildcard is kept, as the broader grant.
                if (decidedHere)
                    return access;

                //Decided once what is beneath it is known: written unless that goes beyond it.
                wildCardAccess = access;
            }
            else if (decidedHere && actionTreeItem.Action == null && actionTreeItem.WildCardAccess.Count > 0)
            {
                //A wildcard is given only by the same wildcard: a group wildcard the reducer holds none of gives nothing,
                //not even on the actions beneath it that the reducer holds one by one.
                return null;
            }
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
                }

                if (subItems.Count() > 0)
                {
                    var dynamicItems = new Dictionary<string, object>();

                    accessTree[actionTreeItem.ID] = dynamicItems;

                    //Each row once, in ordinal order, not in the order of the trees that listed it.
                    var ids = subItems.Select(x => (x.Action as DynamicAction)!.Id!).Distinct().OrderBy(x => x, StringComparer.Ordinal);

                    foreach (var id in ids)
                    {
                        var subActionTreeItem = new ActionTreeNode(actionTreeItem.Path)
                        {
                            Action = actionTreeItem.Action,
                            ID = id
                        };

                        var value = TraverseActionTree(typeAuthContext, subActionTreeItem, dynamicItems, reducer, reducedActionTreeItems, true, preserver, preservedActionTreeItems);

                        if (value != null)
                            dynamicItems![subActionTreeItem.ID] = value;
                    }

                    if (dynamicItems.Count > 0)
                        return dynamicItems;

                    //No row is written: what is left to write is the action's own value, below.
                }
            }

            if (actionTreeItem.Action.Type == ActionType.Text && actionTreeItem.Action is ITextAccessProperties textProps)
            {
                //A row, or the action itself: a dynamic action's own value is its wildcard value.
                var id = actionTreeItem.Action is DynamicAction && stopTraversing ? actionTreeItem.ID : null;

                string? value = typeAuthContext.GetTextAccessValue(actionTreeItem.Action, textProps, id);
                string? heldValue = reducer.GetTextAccessValue(actionTreeItem.Action, textProps, id);

                value = ReduceValue(textProps, value, heldValue);

                //What of the stored value the saver couldn't have given is kept.
                if (preserver != null)
                    value = textProps.Combine.Preserve(value, preserver.GetTextAccessValue(actionTreeItem.Action, textProps, id), heldValue, textProps.MinimumAccess, textProps.MaximumAccess);

                if (value == null || value == textProps.MinimumAccess)
                    return null;

                if (actionTreeItem.Action is DecimalAction || actionTreeItem.Action is DynamicDecimalAction)
                    return decimal.Parse(value, CultureInfo.InvariantCulture);

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

        if (wildCardAccess != null && !GoesBeyond(accessTree, wildCardAccess))
            return wildCardAccess;

        if (accessTree.Count() == 0)
            return null;

        return accessTree;
    }

    //Whether a group's wildcard is written as it is, even when what is beneath it goes further.
    //A context that is its own reducer, with nothing to preserve, is written out for what it grants. Built from several
    //trees, it can hold one tree's wildcard on a group and another tree's grants beneath it, and Can grants both. A JSON
    //node holds only one of them, so there the wildcard is written only when nothing beneath it goes beyond it.
    //Elsewhere the wildcard wins, as before. One tree cannot hold both, so in a context built from one tree, grants
    //beneath a wildcard mean an edit: editors change WildCardAccess in place, and their wildcard wins. With another
    //reducer or a preserver, the wildcard wins as far as the reducer holds it as a wildcard at the node: a wildcard is
    //given only by a wildcard, and grants the reducer holds on the actions beneath it don't count.
    private static bool WildCardWins(TypeAuthContext typeAuthContext, TypeAuthContext reducer, TypeAuthContext? preserver)
    {
        return !ReferenceEquals(reducer, typeAuthContext) || preserver != null || typeAuthContext.AccessTreeJsonStrings.Count < 2;
    }

    //Whether a value written beneath a wildcard grants more than the wildcard: an access the wildcard lacks, or a text
    //value when the wildcard lacks Maximum (such a wildcard sets text actions to their minimum, which is never written).
    private static bool GoesBeyond(object value, List<Access> wildCardAccess)
    {
        if (value is List<Access> accesses)
            return accesses.Any(x => !wildCardAccess.Contains(x));

        if (value is Dictionary<string, object> items)
            return items.Values.Any(x => GoesBeyond(x, wildCardAccess));

        return !wildCardAccess.Contains(Access.Maximum);
    }

    //Whether the stored grants at or beneath the node hold a wildcard, an access, a row or a text value that the reducer
    //can't grant and that the wildcard being written doesn't give.
    private static bool StoredGoesBeyond(TypeAuthContext preserver, TypeAuthContext reducer, List<ActionTreeNode> reducedActionTreeItems, ActionTreeNode node, List<Access> wildCard)
    {
        //A stored wildcard on the node or a group beneath it, compared with the reducer's wildcard there, as a save keeps
        //one on the node itself. Its Maximum counts: it gives text actions added to the group later their maximum.
        foreach (var group in NodesAt(node))
        {
            var held = reducedActionTreeItems.FirstOrDefault(x => x.Path == group.Path)?.WildCardAccess ?? new List<Access>();

            if (group.WildCardAccess.Any(x => !held.Contains(x) && !wildCard.Contains(x)))
                return true;
        }

        foreach (var action in ActionsAt(node))
        {
            var rows = action is DynamicAction dynamicAction
                ? preserver.TypeAuthContextHelper.LocateActionInBank(dynamicAction).SelectMany(x => x.SubActionBankItems).Select(x => (x.Action as DynamicAction)!.Id).Distinct().ToList()
                : new List<string?>();

            if (action is ITextAccessProperties textProps)
            {
                var given = wildCard.Contains(Access.Maximum) ? textProps.MaximumAccess : textProps.MinimumAccess;

                foreach (var id in new string?[] { null }.Concat(rows))
                {
                    var stored = preserver.GetTextAccessValue(action, textProps, id);

                    if (!textProps.Combine.Within(reducer.GetTextAccessValue(action, textProps, id), stored, textProps.MinimumAccess, textProps.MaximumAccess) &&
                        !textProps.Combine.Within(given, stored, textProps.MinimumAccess, textProps.MaximumAccess))
                        return true;
                }

                continue;
            }

            //The accesses an action of its type can be given, as the assignment check counts them.
            foreach (var access in AccessTreeComparer.Grantable(action).Where(x => !wildCard.Contains(x)))
            {
                if (preserver.Can(action, access) && !reducer.Can(action, access))
                    return true;

                if (rows.Any(id => preserver.Can(action, access, id) && !reducer.Can(action, access, id)))
                    return true;
            }
        }

        return false;
    }

    //The node and every node beneath it, other than a dynamic action's rows.
    private static IEnumerable<ActionTreeNode> NodesAt(ActionTreeNode node)
    {
        yield return node;

        foreach (var child in node.ActionTreeItems.Where(x => !x.IsADynamicSubItem))
            foreach (var item in NodesAt(child))
                yield return item;
    }

    //The node's action, or every action beneath it.
    private static IEnumerable<ActionBase> ActionsAt(ActionTreeNode node)
    {
        if (node.Action != null)
        {
            yield return node.Action;
            yield break;
        }

        foreach (var child in node.ActionTreeItems.Where(x => !x.IsADynamicSubItem))
            foreach (var action in ActionsAt(child))
                yield return action;
    }
}
