using ShiftSoftware.TypeAuth.Core.Actions;

namespace ShiftSoftware.TypeAuth.Core
{
    internal class TypeAuthContextHelper
    {
        private readonly ActionTreeBuilder _actionTreeBuilder;
        private readonly ActionBankPopulator _actionBankPopulator;

        internal List<ActionBankItem> ActionBank => _actionBankPopulator.ActionBank;

        public TypeAuthContextHelper()
        {
            _actionTreeBuilder = new ActionTreeBuilder();
            _actionBankPopulator = new ActionBankPopulator();
        }

        internal ActionTreeNode GenerateActionTree(List<Type> actionTrees, List<string> accessTreeJSONStrings, ActionTreeNode? rootActionTree)
        {
            return _actionTreeBuilder.GenerateActionTree(actionTrees, accessTreeJSONStrings, rootActionTree);
        }

        internal void PopulateActionBank(ActionTreeNode actionCursor, object? accessCursor)
        {
            _actionBankPopulator.PopulateActionBank(actionCursor, accessCursor);
        }

        internal void ExpandDynamicActions(ActionTreeNode actionTreeItem)
        {
            _actionBankPopulator.ExpandDynamicActions(actionTreeItem);
        }

        internal IEnumerable<ActionBankItem> LocateActionInBank(ActionBase actionToCheck, string? Id = null, params string[]? selfId)
        {
            foreach (var item in this.ActionBank)
            {
                if (item.Action.Path != actionToCheck.Path)
                    continue;

                if (IsMatch(item, Id))
                    yield return item;

                if (actionToCheck is DynamicAction)
                {
                    foreach (var sub in item.SubActionBankItems)
                    {
                        var action = (sub.Action as DynamicAction)!;

                        if (action.Id == Id || (Id != null && selfId != null && selfId.Contains(Id) && action.Id == TypeAuthContext.SelfReferenceKey))
                        {
                            if (IsMatch(sub, Id))
                                yield return sub;
                        }
                    }
                }
            }
        }

        internal bool Can(ActionBase actionToCheck, Access accessTypeToCheck, string? Id = null, params string[]? selfId)
        {
            return LocateActionInBank(actionToCheck, Id, selfId)
                .Any(x => x.AccessList.IndexOf(accessTypeToCheck) > -1);
        }

        private static bool IsMatch(ActionBankItem item, string? Id)
        {
            var dynamicAction = item.Action as DynamicAction;

            if (Id != null && dynamicAction != null && dynamicAction.Id == null && item.AccessList.Count == 0)
                return false;

            return true;
        }
    }
}
