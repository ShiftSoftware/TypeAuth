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

        internal List<ActionBankItem> LocateActionInBank(ActionBase actionToCheck, string? Id = null, params string[]? selfId)
        {
            List<ActionBankItem> actionMatches = new List<ActionBankItem> { };

            foreach (var item in this.ActionBank.Where(x => x.Action.Path == actionToCheck.Path).ToList())
            {
                actionMatches.Add(item);

                if (actionToCheck is DynamicAction)
                {
                    actionMatches.AddRange(item.SubActionBankItems.Where(x =>
                    {
                        var action = (x.Action as DynamicAction)!;

                        return action.Id == Id || (Id != null && selfId != null && selfId.Contains(Id) && action.Id == TypeAuthContext.SelfReferenceKey);
                    }));
                }
            }

            return actionMatches
                .Where(x =>
                {
                    var dynamicAction = x.Action as DynamicAction;

                    if (Id != null && dynamicAction != null && dynamicAction.Id == null && x.AccessList.Count == 0)
                        return false;

                    return true;
                }).ToList();
        }

        internal bool Can(ActionBase actionToCheck, Access accessTypeToCheck, string? Id = null, params string[]? selfId)
        {
            var access = false;

            var actionMatches = this.LocateActionInBank(actionToCheck, Id, selfId);

            access = actionMatches.Any(actionMatch => actionMatch.AccessList.IndexOf(accessTypeToCheck) > -1);

            return access;
        }
    }
}
