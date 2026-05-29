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
}
