
namespace ShiftSoftware.TypeAuth.Core;

public static class TypeAuthContextExtensions
{
    public static bool Can(this TypeAuthContext typeAuthContext, Type actionTreeType, string actionName, Access access)
    {
        var action = (Core.Actions.Action)actionTreeType
            .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .FirstOrDefault(x => x.Name.Equals(actionName, StringComparison.InvariantCultureIgnoreCase))
            ?.GetValue(null)!;

        if (action == null)
            throw new Exception($"No Such Action ({actionTreeType.FullName}.{actionName})");

        return typeAuthContext.TypeAuthContextHelper.Can(action, access);
    }
}