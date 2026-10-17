using ShiftSoftware.TypeAuth.Core.Actions;

namespace ShiftSoftware.TypeAuth.Core
{
    internal class ActionBankPopulator
    {
        internal List<ActionBankItem> ActionBank { get; set; }

        public ActionBankPopulator()
        {
            ActionBank = new List<ActionBankItem>();
        }

        internal void PopulateActionBank(ActionTreeNode actionCursor, object? accessCursor)
        {
            if (actionCursor.IsADynamicSubItem)
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
                        MergeWildCardAccess(actionCursor, node.AccessArray);
                        this.PopulateActionBank(entry, accessCursor);
                    }
                    else
                    {
                        if (accessCursor != null)
                            PopulateActionBank(entry, node.AccessObject![entry.ID]);
                    }
                }
            }

            if (actionCursor.Action != null && (node.AccessArray.Count > 0 || node.AccessValue != null || (actionCursor.Action is DynamicAction && node.AccessObject != null)))
            {
                var theAction = (ActionBase)actionCursor.Action;

                if (theAction.Type == ActionType.Text && node.AccessValue == null && theAction is ITextAccessProperties textProps)
                {
                    if (node.AccessArray.Contains(Access.Maximum))
                        node.AccessValue = textProps.MaximumAccess;
                    else
                        node.AccessValue = textProps.MinimumAccess;
                }

                if (theAction  is DynamicAction && node.AccessArray.Count > 0)
                    MergeWildCardAccess(actionCursor, node.AccessArray);

                this.ActionBank.Add(new ActionBankItem(theAction, node.AccessArray, node.AccessValue, node.AccessObject));
            }
        }

        //Several access trees can grant a wildcard on the same node. The node keeps the accesses of all of them, as Can
        //does, so that it does not depend on the order the trees were added in.
        private static void MergeWildCardAccess(ActionTreeNode actionCursor, List<Access> accessArray)
        {
            //The first wildcard keeps its list. On a dynamic action that list is also the action bank item's access list,
            //and an editor that toggles one toggles the other.
            if (actionCursor.WildCardAccess.Count == 0)
            {
                actionCursor.WildCardAccess = accessArray;
                return;
            }

            if (accessArray.All(actionCursor.WildCardAccess.Contains))
                return;

            //A new list, so that the first tree's action bank item does not take the other trees' accesses.
            actionCursor.WildCardAccess = actionCursor.WildCardAccess.Union(accessArray).ToList();
        }

        internal void ExpandDynamicActions(ActionTreeNode actionTreeItem)
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
                    var newTreeItem = new ActionTreeNode(actionTreeItem.Path + item.Key)
                    {
                        Action = action,
                        DisplayName = item.Value,
                        ID = item.Key,
                        WildCardAccess = new List<Access>(),
                        IsADynamicSubItem = true
                    };

                    actionTreeItem.ActionTreeItems.Add(newTreeItem);
                }
            }
        }
    }
}
