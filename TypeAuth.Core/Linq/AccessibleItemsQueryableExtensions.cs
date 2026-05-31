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
    /// A row is kept when <em>any one</em> of <paramref name="idSelectors"/> resolves to an accessible id.
    /// </summary>
    /// <remarks>
    /// When <see cref="AccessibleItemsResult.WildCard"/> is <see langword="true"/> the query is returned unchanged
    /// (the caller can access everything). Otherwise the accessible string IDs are converted to
    /// <typeparamref name="TKey"/> via <paramref name="idConverter"/> and matched against the selected columns. The
    /// reserved <see cref="TypeAuthContext.EmptyOrNullKey"/> maps to <see langword="null"/>, so rows whose key is
    /// <see langword="null"/> on any selected column are included when the caller has been granted access to the
    /// empty/null key. Pass more than one selector for an OR across columns (e.g. owner + intermediary).
    /// </remarks>
    /// <typeparam name="T">The queried entity type.</typeparam>
    /// <typeparam name="TKey">The underlying (non-nullable) key type, e.g. <see cref="long"/>.</typeparam>
    /// <param name="query">The source query.</param>
    /// <param name="accessible">The accessibility result describing which IDs the caller can reach.</param>
    /// <param name="idConverter">Converts an accessible string ID into <typeparamref name="TKey"/>.</param>
    /// <param name="idSelectors">
    /// One or more (nullable) key selectors (a <see langword="params"/> array), e.g.
    /// <c>WhereAccessible(accessible, long.Parse, x =&gt; x.CompanyID, x =&gt; x.IntermediaryCompanyID)</c>.
    /// At least one is required.
    /// </param>
    /// <returns>The filtered query, or the original query when access is wildcard.</returns>
    public static IQueryable<T> WhereAccessible<T, TKey>(
        this IQueryable<T> query,
        AccessibleItemsResult accessible,
        Func<string, TKey> idConverter,
        params Expression<Func<T, TKey?>>[] idSelectors)
        where TKey : struct
    {
        return query.WhereIn(accessible.ConvertIds(idConverter), idSelectors);
    }

    /// <summary>
    /// Filters <paramref name="query"/> so that the value produced by <em>any one</em> of
    /// <paramref name="selectors"/> is contained in <paramref name="values"/> — i.e.
    /// <c>sel1(x) ∈ values || sel2(x) ∈ values || …</c>. Pass a single selector for a plain
    /// <c>column IN values</c>; pass several for an OR across columns.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When <paramref name="values"/> is <see langword="null"/> the query is returned unchanged. This mirrors the
    /// "wildcard = all items accessible" convention of <see cref="AccessibleItemsResult.ConvertIds{TKey}"/>, which
    /// returns <see langword="null"/> for a wildcard grant. An empty list, by contrast, filters out everything. A
    /// <see langword="null"/> entry in <paramref name="values"/> matches rows whose value is <see langword="null"/>
    /// on any one of the columns (per-column).
    /// </para>
    /// <para>
    /// The filter is a single <c>Expression.Constant</c> over <paramref name="values"/> plus one <c>List.Contains</c>
    /// call per selector, OR-combined with <c>Expression.OrElse</c> and wrapped in one <c>Where</c> — so it
    /// translates to SQL as one <c>IN (…)</c> per column joined by <c>OR</c>. Each selector is authored independently
    /// and so carries its own lambda parameter; the bodies are rebound onto one shared parameter so they compose
    /// into a single valid <c>Where</c> lambda.
    /// </para>
    /// </remarks>
    /// <typeparam name="T">The queried entity type.</typeparam>
    /// <typeparam name="TValue">The compared value type.</typeparam>
    /// <param name="query">The source query.</param>
    /// <param name="values">The allowed values, or <see langword="null"/> to apply no filter.</param>
    /// <param name="selectors">
    /// One or more column selectors (a <see langword="params"/> array), e.g.
    /// <c>WhereIn(values, x =&gt; x.CompanyID, x =&gt; x.IntermediaryCompanyID)</c>. At least one is required.
    /// </param>
    /// <returns>The filtered query, or the original query when <paramref name="values"/> is <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="selectors"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="selectors"/> is empty.</exception>
    public static IQueryable<T> WhereIn<T, TValue>(
        this IQueryable<T> query,
        List<TValue>? values,
        params Expression<Func<T, TValue>>[] selectors)
    {
        if (selectors is null)
            throw new ArgumentNullException(nameof(selectors));

        // Empty selectors is a misconfiguration, not a data condition: "is any of zero columns in this set?" has no
        // sensible answer. Fail loud rather than silently match everything (fail-open) or nothing — this primitive
        // backs data-level access, where a silent fail-open would leak rows.
        if (selectors.Length == 0)
            throw new ArgumentException("WhereIn requires at least one selector.", nameof(selectors));

        if (values is null)
            return query;

        var parameter = Expression.Parameter(typeof(T), "x");
        var valuesConstant = Expression.Constant(values);
        var containsMethod = typeof(List<TValue>).GetMethod(nameof(List<TValue>.Contains), new[] { typeof(TValue) })!;

        Expression? predicate = null;
        foreach (var selector in selectors)
        {
            // Each selector has its own parameter; rebind its body onto the shared parameter so the disjuncts
            // combine into one lambda (and translate as a single Where).
            var selectorBody = new ParameterReplaceVisitor(selector.Parameters[0], parameter).Visit(selector.Body);
            var containsCall = Expression.Call(valuesConstant, containsMethod, selectorBody);
            predicate = predicate is null ? containsCall : Expression.OrElse(predicate, containsCall);
        }

        return query.Where(Expression.Lambda<Func<T, bool>>(predicate!, parameter));
    }

    /// <summary>
    /// Rewrites references to one <see cref="ParameterExpression"/> as another, so independently-authored selector
    /// lambdas can be OR-combined under a single shared parameter.
    /// </summary>
    private sealed class ParameterReplaceVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression from;
        private readonly ParameterExpression to;

        public ParameterReplaceVisitor(ParameterExpression from, ParameterExpression to)
        {
            this.from = from;
            this.to = to;
        }

        protected override Expression VisitParameter(ParameterExpression node)
            => node == from ? to : base.VisitParameter(node);
    }
}
