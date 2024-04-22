
using ShiftSoftware.TypeAuth.Core.Actions;

namespace ShiftSoftware.TypeAuth.Core;

public interface ITypeAuthService
{
    bool Can(ActionBase action, Access access);

    bool Can(ActionBase action, Access access, string Id, params string[]? selfId);

    bool Can(Type actionTreeType, string actionName, Access access);

    bool CanRead(ReadAction action);

    bool CanRead(DynamicReadAction action, string Id, params string[]? selfId);

    bool CanRead(ReadWriteAction action);

    bool CanRead(DynamicReadWriteAction action, string Id, params string[]? selfId);

    bool CanRead(ReadWriteDeleteAction action);

    bool CanRead(DynamicReadWriteDeleteAction action, string Id, params string[]? selfId);

    bool CanWrite(ReadWriteAction action);

    bool CanWrite(DynamicReadWriteAction action, string Id, params string[]? selfId);

    bool CanWrite(ReadWriteDeleteAction action);

    bool CanWrite(DynamicReadWriteDeleteAction action, string Id, params string[]? selfId);

    bool CanDelete(ReadWriteDeleteAction action);

    bool CanDelete(DynamicReadWriteDeleteAction action, string Id, params string[]? selfId);

    bool CanAccess(BooleanAction action);

    bool CanAccess(DynamicBooleanAction action, string Id, params string[]? selfId);

    string? AccessValue(TextAction action);

    decimal? AccessValue(DecimalAction action);

    string? AccessValue(DynamicTextAction action, string? Id, params string[]? selfId);

    decimal? AccessValue(DynamicDecimalAction action, string? Id, params string[]? selfId);

    void SetAccessValue(TextAction theAction, string? value, string? maximumValue);

    void SetAccessValue(DynamicTextAction theAction, string? Id, string? value, string? maximumValue);

    void ToggleAccess(ActionBase theAction, Access access, string? Id = null);

    string GenerateAccessTree(TypeAuthContext reducer, TypeAuthContext? preserver = null);

    Type[] GetRegisteredActionTrees();

    Dictionary<ActionBase, string> FindInAccessibleActionsOn(TypeAuthContext typeAuthContextToCompare);

    (bool WildCard, List<string> AccessibleIds) GetAccessibleItems(DynamicAction dynamicAction, Func<Access, bool> predicate, params string[]? selfId);
}