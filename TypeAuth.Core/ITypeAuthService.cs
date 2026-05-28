
using ShiftSoftware.TypeAuth.Core.Actions;

namespace ShiftSoftware.TypeAuth.Core;

/// <summary>
/// Service interface for evaluating permissions against an access tree. Mirrors the public API of <see cref="TypeAuthContext"/>.
/// </summary>
public interface ITypeAuthService
{
    /// <summary>
    /// Checks whether the given action has the specified access level.
    /// </summary>
    bool Can(ActionBase action, Access access);

    /// <inheritdoc cref="Can(ActionBase, Access)"/>
    /// <param name="action">The action to check.</param>
    /// <param name="access">The access level to check for.</param>
    /// <param name="Id">The data item ID for dynamic actions. When null, checks access for unassigned/ownerless items.</param>
    /// <param name="selfId">IDs that should be treated as "self" for self-reference resolution.</param>
    bool Can(ActionBase action, Access access, string? Id, params string[]? selfId);

    /// <summary>
    /// Checks whether the given action has Read access.
    /// </summary>
    bool CanRead(ReadAction action);

    /// <inheritdoc cref="CanRead(ReadAction)"/>
    bool CanRead(DynamicReadAction action, string? Id, params string[]? selfId);

    /// <inheritdoc cref="CanRead(ReadAction)"/>
    bool CanRead(ReadWriteAction action);

    /// <inheritdoc cref="CanRead(ReadAction)"/>
    bool CanRead(DynamicReadWriteAction action, string? Id, params string[]? selfId);

    /// <inheritdoc cref="CanRead(ReadAction)"/>
    bool CanRead(ReadWriteDeleteAction action);

    /// <inheritdoc cref="CanRead(ReadAction)"/>
    bool CanRead(DynamicReadWriteDeleteAction action, string? Id, params string[]? selfId);

    /// <summary>
    /// Checks whether the given action has Write access.
    /// </summary>
    bool CanWrite(ReadWriteAction action);

    /// <inheritdoc cref="CanWrite(ReadWriteAction)"/>
    bool CanWrite(DynamicReadWriteAction action, string? Id, params string[]? selfId);

    /// <inheritdoc cref="CanWrite(ReadWriteAction)"/>
    bool CanWrite(ReadWriteDeleteAction action);

    /// <inheritdoc cref="CanWrite(ReadWriteAction)"/>
    bool CanWrite(DynamicReadWriteDeleteAction action, string? Id, params string[]? selfId);

    /// <summary>
    /// Checks whether the given action has Delete access.
    /// </summary>
    bool CanDelete(ReadWriteDeleteAction action);

    /// <inheritdoc cref="CanDelete(ReadWriteDeleteAction)"/>
    bool CanDelete(DynamicReadWriteDeleteAction action, string? Id, params string[]? selfId);

    /// <summary>
    /// Checks whether the given boolean action is granted.
    /// </summary>
    bool CanAccess(BooleanAction action);

    /// <inheritdoc cref="CanAccess(BooleanAction)"/>
    bool CanAccess(DynamicBooleanAction action, string? Id, params string[]? selfId);

    /// <summary>
    /// Returns the computed text access value for the given action.
    /// </summary>
    string? AccessValue(TextAction action);

    /// <summary>
    /// Returns the computed decimal access value for the given action.
    /// </summary>
    decimal? AccessValue(DecimalAction action);

    /// <inheritdoc cref="AccessValue(TextAction)"/>
    string? AccessValue(DynamicTextAction action, string? Id, params string[]? selfId);

    /// <inheritdoc cref="AccessValue(DecimalAction)"/>
    decimal? AccessValue(DynamicDecimalAction action, string? Id, params string[]? selfId);

    /// <summary>
    /// Returns the action tree types that were registered when building this context.
    /// </summary>
    Type[] GetRegisteredActionTrees();

    /// <summary>
    /// Returns the list of data item IDs accessible for a dynamic action, or indicates wildcard access.
    /// </summary>
    /// <param name="dynamicAction">The dynamic action to query.</param>
    /// <param name="predicate">Filter applied to each access level to determine if an item is accessible.</param>
    /// <param name="selfId">IDs that should be treated as "self" for self-reference resolution.</param>
    AccessibleItemsResult GetAccessibleItems(DynamicAction dynamicAction, Func<Access, bool> predicate, params string[]? selfId);

    /// <summary>
    /// Returns the items the user can Read for the given dynamic action.
    /// </summary>
    AccessibleItemsResult GetReadableItems(DynamicReadAction action, params string[]? selfId);

    /// <inheritdoc cref="GetReadableItems(DynamicReadAction, string[])"/>
    AccessibleItemsResult GetReadableItems(DynamicReadWriteAction action, params string[]? selfId);

    /// <inheritdoc cref="GetReadableItems(DynamicReadAction, string[])"/>
    AccessibleItemsResult GetReadableItems(DynamicReadWriteDeleteAction action, params string[]? selfId);

    /// <summary>
    /// Returns the items the user can Write for the given dynamic action.
    /// </summary>
    AccessibleItemsResult GetWritableItems(DynamicReadWriteAction action, params string[]? selfId);

    /// <inheritdoc cref="GetWritableItems(DynamicReadWriteAction, string[])"/>
    AccessibleItemsResult GetWritableItems(DynamicReadWriteDeleteAction action, params string[]? selfId);

    /// <summary>
    /// Returns the items the user can Delete for the given dynamic action.
    /// </summary>
    AccessibleItemsResult GetDeletableItems(DynamicReadWriteDeleteAction action, params string[]? selfId);
}
