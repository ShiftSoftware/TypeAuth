namespace ShiftSoftware.TypeAuth.Core;

public class AccessibleItemsByAccess
{
    public AccessibleItemsResult Read { get; }
    public AccessibleItemsResult Write { get; }
    public AccessibleItemsResult Delete { get; }
    public AccessibleItemsResult Maximum { get; }

    public AccessibleItemsByAccess(
        AccessibleItemsResult read,
        AccessibleItemsResult write,
        AccessibleItemsResult delete,
        AccessibleItemsResult maximum)
    {
        Read = read;
        Write = write;
        Delete = delete;
        Maximum = maximum;
    }

    /// <summary>
    /// Returns the accessible-items set for a given <see cref="Access"/> level, so a caller can select the level
    /// once and reuse the result instead of branching on <see cref="Access"/> at each call site.
    /// </summary>
    /// <param name="access">The access level to select.</param>
    /// <returns>The matching set: <see cref="Read"/> / <see cref="Write"/> / <see cref="Delete"/> / <see cref="Maximum"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="access"/> is not a defined level.</exception>
    public AccessibleItemsResult For(Access access) => access switch
    {
        Access.Read => Read,
        Access.Write => Write,
        Access.Delete => Delete,
        Access.Maximum => Maximum,
        _ => throw new ArgumentOutOfRangeException(nameof(access), access, "No accessible-items set for this access level."),
    };
}
