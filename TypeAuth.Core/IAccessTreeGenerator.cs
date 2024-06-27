using ShiftSoftware.TypeAuth.Core.Actions;

namespace ShiftSoftware.TypeAuth.Core;

public interface IAccessTreeGenerator
{
    void SetAccessValue(TextAction theAction, string? value, string? maximumValue);

    void SetAccessValue(DynamicTextAction theAction, string? Id, string? value, string? maximumValue);

    void ToggleAccess(ActionBase theAction, Access access, string? Id = null);

    string GenerateAccessTree(TypeAuthContext reducer, TypeAuthContext? preserver = null);

    Type[] GetRegisteredActionTrees();

    Dictionary<ActionBase, string> FindInAccessibleActionsOn(TypeAuthContext typeAuthContextToCompare);
}