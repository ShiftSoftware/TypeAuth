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

    /// <summary>
    /// Row-check twin of the <c>WhereIn</c>/<c>WhereAccessible</c> query appliers: returns <see langword="true"/>
    /// when the caller can reach an entity via <em>any one</em> of its key columns. <see cref="WildCard"/> ⇒ always
    /// <see langword="true"/>; otherwise the accessible ids are decoded with <paramref name="idConverter"/>
    /// (<see cref="TypeAuthContext.EmptyOrNullKey"/> ⇒ <see langword="null"/>, so a <see langword="null"/> key matches)
    /// and the entity is reachable if any of <paramref name="keys"/> is in that set.
    /// </summary>
    /// <remarks>
    /// Takes the <em>same</em> <c>string → TKey</c> converter as <c>WhereAccessible</c> / <see cref="ConvertIds{TKey}"/>,
    /// so one declaration drives both the query and row paths (one source, two emissions). Pass a single key for a plain
    /// membership check; pass several (e.g. owner + intermediary) for an OR.
    /// </remarks>
    /// <typeparam name="TKey">The (non-nullable) key type, e.g. <see cref="long"/>.</typeparam>
    /// <param name="idConverter">Converts an accessible id string into <typeparamref name="TKey"/> (same as the query path).</param>
    /// <param name="keys">One or more (nullable) entity keys (a <see langword="params"/> array). At least one is required.</param>
    /// <returns><see langword="true"/> if wildcard, or if any key is in the accessible set.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="idConverter"/> or <paramref name="keys"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="keys"/> is empty.</exception>
    public bool HasAccessTo<TKey>(Func<string, TKey> idConverter, params TKey?[] keys) where TKey : struct
    {
        if (idConverter is null)
            throw new ArgumentNullException(nameof(idConverter));
        if (keys is null)
            throw new ArgumentNullException(nameof(keys));

        // Mirror WhereIn's fail-closed guard: "does the set contain any of zero keys?" has no sensible answer,
        // and a silent true/false would hide a misconfiguration in a data-level check.
        if (keys.Length == 0)
            throw new ArgumentException("HasAccessTo requires at least one key.", nameof(keys));

        if (WildCard)
            return true;

        var accessible = ConvertIds(idConverter)!; // non-null here: the wildcard case already returned above
        foreach (var key in keys)
            if (accessible.Contains(key))
                return true;

        return false;
    }

    public List<TKey?>? ConvertIds<TKey>(Func<string, TKey> converter) where TKey : struct
    {
        if (WildCard) return null;

        return AccessibleIds
            .Select(id => id == TypeAuthContext.EmptyOrNullKey ? (TKey?)null : converter(id))
            .ToList();
    }
}
