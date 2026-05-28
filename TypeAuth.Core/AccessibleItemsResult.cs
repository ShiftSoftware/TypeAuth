namespace ShiftSoftware.TypeAuth.Core;

public class AccessibleItemsResult
{
    public bool WildCard { get; }
    public List<string> AccessibleIds { get; }

    public AccessibleItemsResult(bool wildCard, List<string> accessibleIds)
    {
        WildCard = wildCard;
        AccessibleIds = accessibleIds;
    }

    public bool HasAccessTo(string id) => WildCard || AccessibleIds.Contains(id);

    public List<TKey?>? ConvertIds<TKey>(Func<string, TKey> converter) where TKey : struct
    {
        if (WildCard) return null;

        return AccessibleIds
            .Select(id => id == TypeAuthContext.EmptyOrNullKey ? (TKey?)null : converter(id))
            .ToList();
    }
}
