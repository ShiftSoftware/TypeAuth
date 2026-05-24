using ShiftSoftware.TypeAuth.Core.Actions;

namespace ShiftSoftware.TypeAuth.Core;

/// <summary>
/// Extension methods for modifying access values and toggling permissions on a <see cref="TypeAuthContext"/>'s action bank.
/// </summary>
public static class AccessTreeEditor
{
    /// <summary>
    /// Sets the text access value for the given action, constrained by the specified maximum.
    /// </summary>
    public static void SetAccessValue(this TypeAuthContext typeAuthContext, TextAction theAction, string? value, string? maximumValue)
    {
        typeAuthContext.SetTextAccessValue(theAction, theAction, value, maximumValue);
    }

    /// <inheritdoc cref="SetAccessValue(TypeAuthContext, TextAction, string, string)"/>
    /// <param name="typeAuthContext">The context to modify.</param>
    /// <param name="theAction">The dynamic text action to set.</param>
    /// <param name="Id">The data item ID.</param>
    /// <param name="value">The value to assign.</param>
    /// <param name="maximumValue">The maximum allowed value.</param>
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

    /// <summary>
    /// Toggles the specified access level on or off for the given action.
    /// </summary>
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
        var actionMatches = typeAuthContext.TypeAuthContextHelper.LocateActionInBank(theAction, Id).ToList();

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

                actionMatches = typeAuthContext.TypeAuthContextHelper.LocateActionInBank(theAction, Id).ToList();
            }


            if (!actionMatches.Any(x => (x.Action as DynamicAction)!.Id == Id))
            {
                actionMatches.First().SubActionBankItems.Add(new ActionBankItem(new DynamicAction { Id = Id, Type = theAction.Type }, new List<Access>()));

                actionMatches = typeAuthContext.TypeAuthContextHelper.LocateActionInBank(theAction, Id).ToList();
            }
        }

        return actionMatches;
    }
}
