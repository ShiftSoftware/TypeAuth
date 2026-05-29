using System.Linq.Expressions;

namespace ShiftSoftware.TypeAuth.Core.Linq;

/// <summary>
/// LINQ extensions that translate TypeAuth accessibility results into <see cref="IQueryable{T}"/> filters.
/// <para>
/// These live in a dedicated namespace so they only surface on <see cref="IQueryable{T}"/> when explicitly
/// imported with <c>using ShiftSoftware.TypeAuth.Core.Linq;</c> — they don't pollute the root namespace.
/// </para>
/// </summary>
public static class AccessibleItemsQueryableExtensions
{
    /// <summary>
    /// Filters <paramref name="query"/> down to the rows the caller is allowed to reach, based on an
    /// <see cref="AccessibleItemsResult"/> produced by the <c>GetAccessibleItems</c>/<c>GetReadableItems</c> family.
    /// </summary>
    /// <remarks>
    /// When <see cref="AccessibleItemsResult.WildCard"/> is <see langword="true"/> the query is returned unchanged
    /// (the caller can access everything). Otherwise the accessible string IDs are converted to
    /// <typeparamref name="TKey"/> via <paramref name="idConverter"/> and matched against <paramref name="idSelector"/>.
    /// The reserved <see cref="TypeAuthContext.EmptyOrNullKey"/> maps to <see langword="null"/>, so rows whose key is
    /// <see langword="null"/> are included when the caller has been granted access to the empty/null key.
    /// </remarks>
    /// <typeparam name="T">The queried entity type.</typeparam>
    /// <typeparam name="TKey">The underlying (non-nullable) key type, e.g. <see cref="long"/>.</typeparam>
    /// <param name="query">The source query.</param>
    /// <param name="accessible">The accessibility result describing which IDs the caller can reach.</param>
    /// <param name="idSelector">Selects the (nullable) key from an entity, e.g. <c>x =&gt; x.CountryID</c>.</param>
    /// <param name="idConverter">Converts an accessible string ID into <typeparamref name="TKey"/>.</param>
    /// <returns>The filtered query, or the original query when access is wildcard.</returns>
    public static IQueryable<T> WhereAccessible<T, TKey>(
        this IQueryable<T> query,
        AccessibleItemsResult accessible,
        Expression<Func<T, TKey?>> idSelector,
        Func<string, TKey> idConverter)
        where TKey : struct
    {
        return query.WhereIn(idSelector, accessible.ConvertIds(idConverter));
    }

    /// <summary>
    /// Filters <paramref name="query"/> so that the value produced by <paramref name="selector"/> is contained in
    /// <paramref name="values"/>.
    /// </summary>
    /// <remarks>
    /// When <paramref name="values"/> is <see langword="null"/> the query is returned unchanged. This mirrors the
    /// "wildcard = all items accessible" convention of <see cref="AccessibleItemsResult.ConvertIds{TKey}"/>, which
    /// returns <see langword="null"/> for a wildcard grant. An empty list, by contrast, filters out everything.
    /// </remarks>
    /// <typeparam name="T">The queried entity type.</typeparam>
    /// <typeparam name="TValue">The compared value type.</typeparam>
    /// <param name="query">The source query.</param>
    /// <param name="selector">Selects the value to compare, e.g. <c>x =&gt; x.CountryID</c>.</param>
    /// <param name="values">The allowed values, or <see langword="null"/> to apply no filter.</param>
    /// <returns>The filtered query, or the original query when <paramref name="values"/> is <see langword="null"/>.</returns>
    public static IQueryable<T> WhereIn<T, TValue>(
        this IQueryable<T> query,
        Expression<Func<T, TValue>> selector,
        List<TValue>? values)
    {
        if (values is null)
            return query;

        var containsCall = Expression.Call(
            Expression.Constant(values),
            typeof(List<TValue>).GetMethod(nameof(List<TValue>.Contains), new[] { typeof(TValue) })!,
            selector.Body);

        return query.Where(Expression.Lambda<Func<T, bool>>(containsCall, selector.Parameters));
    }
}
