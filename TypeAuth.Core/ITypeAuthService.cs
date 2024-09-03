
using ShiftSoftware.TypeAuth.Core.Actions;

namespace ShiftSoftware.TypeAuth.Core;

public interface ITypeAuthService
{
    bool Can(ActionBase action, Access access);

    bool Can(ActionBase action, Access access, string Id, params string[]? selfId);


    bool CanRead<TActionTree>(Func<TActionTree, ReadAction> action);

    bool CanRead<TActionTree>(Func<TActionTree, DynamicReadAction> action, string Id, params string[]? selfId);

    bool CanRead<TActionTree>(Func<TActionTree, ReadWriteAction> action);

    bool CanRead<TActionTree>(Func<TActionTree, DynamicReadWriteAction> action, string Id, params string[]? selfId);

    bool CanRead<TActionTree>(Func<TActionTree, ReadWriteDeleteAction> action);

    bool CanRead<TActionTree>(Func<TActionTree, DynamicReadWriteDeleteAction> action, string Id, params string[]? selfId);

    bool CanWrite<TActionTree>(Func<TActionTree, ReadWriteAction> action);

    bool CanWrite<TActionTree>(Func<TActionTree, DynamicReadWriteAction> action, string Id, params string[]? selfId);

    bool CanWrite<TActionTree>(Func<TActionTree, ReadWriteDeleteAction> action);

    bool CanWrite<TActionTree>(Func<TActionTree, DynamicReadWriteDeleteAction> action, string Id, params string[]? selfId);

    bool CanDelete<TActionTree>(Func<TActionTree, ReadWriteDeleteAction> action);

    bool CanDelete<TActionTree>(Func<TActionTree, DynamicReadWriteDeleteAction> action, string Id, params string[]? selfId);

    bool CanAccess<TActionTree>(Func<TActionTree, BooleanAction> action);

    bool CanAccess<TActionTree>(Func<TActionTree, DynamicBooleanAction> action, string Id, params string[]? selfId);

    string? AccessValue<TActionTree>(Func<TActionTree, TextAction> action);

    decimal? AccessValue<TActionTree>(Func<TActionTree, DecimalAction> action);

    string? AccessValue<TActionTree>(Func<TActionTree, DynamicTextAction> action, string? Id, params string[]? selfId);

    decimal? AccessValue<TActionTree>(Func<TActionTree, DynamicDecimalAction> action, string? Id, params string[]? selfId);

    Type[] GetRegisteredActionTrees();

    (bool WildCard, List<string> AccessibleIds) GetAccessibleItems<TActionTree>(Func<TActionTree, DynamicAction> dynamicAction, Func<Access, bool> predicate, params string[]? selfId);
}