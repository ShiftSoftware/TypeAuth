using ShiftSoftware.TypeAuth.Core.Actions;

namespace ShiftSoftware.TypeAuth.Core;

public static class AccessTreeEditor
{
    public static void SetAccessValue(this TypeAuthContext typeAuthContext, TextAction theAction, string? value, string? maximumValue)
    {
        typeAuthContext.SetTextAccessValue(theAction, theAction, value, maximumValue);
    }

    public static void SetAccessValue(this TypeAuthContext typeAuthContext, DynamicTextAction theAction, string? Id, string? value, string? maximumValue)
    {
        typeAuthContext.SetTextAccessValue(theAction, theAction, value, maximumValue, Id);
    }

    private static void SetTextAccessValue(this TypeAuthContext typeAuthContext, ActionBase theAction, ITextAccessProperties textProps, string? valueToSet, string? maxAllowed, string? Id = null)
    {

        var actionMatches = typeAuthContext.FindOrAddInActionBank(theAction, Id);

        foreach (var action in actionMatches)
        {
            valueToSet = AccessTreeTraverser.ReduceValue(textProps, valueToSet, maxAllowed);

            action.AccessValue = valueToSet;
        }
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
}
