using ShiftSoftware.TypeAuth.Core.Actions;

namespace ShiftSoftware.TypeAuth.Core;

/// <summary>
/// Extension methods for comparing two <see cref="TypeAuthContext"/> instances to find permission gaps.
/// </summary>
public static class AccessTreeComparer
{
    /// <summary>
    /// Returns the actions and access levels present in <paramref name="typeAuthContextToCompare"/> but not granted in this context.
    /// </summary>
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
