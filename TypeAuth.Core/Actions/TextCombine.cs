using System.Globalization;

namespace ShiftSoftware.TypeAuth.Core.Actions;

/// <summary>
/// How a text action's values are combined when a user holds several access trees. The same rule decides what a saver
/// can give: saving a tree keeps what the saver's value covers, keeps a stored value the saver couldn't have given, and
/// assigning a tree is allowed only when nothing of it would be dropped.
/// </summary>
/// <remarks>
/// Every rule only widens: a user with more trees never holds less. Delegated administration depends on that.
/// </remarks>
public sealed class TextCombine
{
    private readonly Func<string?, string?, string?>? winner;
    private readonly Func<string, string, int>? order;
    private readonly string? separator;
    private readonly Func<string, string, bool>? covers;

    private TextCombine(Func<string?, string?, string?> winner, Func<string, string, int>? order = null)
    {
        this.winner = winner;
        this.order = order;
    }

    private TextCombine(string separator, Func<string, string, bool> covers)
    {
        this.separator = separator;
        this.covers = covers;
    }

    /// <summary>
    /// One value wins. <paramref name="winner"/> returns the greater of two values and must not depend on their order:
    /// <c>winner(a, b)</c> and <c>winner(b, a)</c> give the same value. Either value can be null.
    /// A value is within the saver's when the saver's value wins over it.
    /// </summary>
    /// <remarks>Check the order independence in your own tests with <see cref="TextCombineAssert.OrderIndependent(TextAction, string?[])"/>.</remarks>
    public static TextCombine Pick(Func<string?, string?, string?> winner)
    {
        if (winner is null)
            throw new ArgumentNullException(nameof(winner));

        return new TextCombine(winner);
    }

    /// <summary>
    /// A list of entries joined by <paramref name="separator"/>, combined as a union.
    /// <paramref name="covers"/>(held, entry) says whether an entry the saver holds covers another entry,
    /// for example a path covers itself and every path under it.
    /// </summary>
    /// <remarks>
    /// The action's minimum access is "nothing" and is left out of lists. Its maximum access covers every entry.
    /// </remarks>
    public static TextCombine List(string separator, Func<string, string, bool> covers)
    {
        if (string.IsNullOrEmpty(separator))
            throw new ArgumentException("A list needs a separator.", nameof(separator));

        if (covers is null)
            throw new ArgumentNullException(nameof(covers));

        return new TextCombine(separator, covers);
    }

    /// <summary>
    /// The greatest decimal wins. Values are parsed with the invariant culture. This is what <see cref="DecimalAction"/> uses.
    /// </summary>
    public static TextCombine MaxDecimal { get; } = new TextCombine(MaxOfDecimals, CompareDecimals);

    /// <summary>
    /// A list of entries joined by <paramref name="separator"/>, combined as a union, where an entry covers only itself
    /// (and the action's maximum covers every entry).
    /// </summary>
    public static TextCombine UnionList(string separator) => List(separator, (held, entry) => string.Equals(held, entry, StringComparison.Ordinal));

    internal bool IsList => this.separator != null;

    internal Func<string?, string?, string?>? Winner => this.winner;

    /// <summary>
    /// The value of several trees: each non-null value combined with the next. A single value is returned as it is.
    /// </summary>
    internal string? Fold(IEnumerable<string?> values, string? minimum, string? maximum)
    {
        string? result = null;
        var any = false;

        foreach (var value in values)
        {
            if (value == null)
                continue;

            result = any ? Combine(result, value, minimum, maximum) : value;
            any = true;
        }

        return any ? result : minimum;
    }

    internal string? Combine(string? first, string? second, string? minimum, string? maximum)
    {
        if (this.winner != null)
        {
            if (first == null)
                return second;

            if (second == null)
                return first;

            return this.winner(first, second);
        }

        //A union of entries, each once, in ordinal order, not in the order of the trees.
        var entries = Entries(first, minimum).Concat(Entries(second, minimum)).Distinct(StringComparer.Ordinal).ToList();

        if (entries.Count == 0)
            return minimum;

        if (entries.Any(x => IsMaximum(x, maximum)))
            return maximum;

        return string.Join(this.separator, entries.OrderBy(x => x, StringComparer.Ordinal));
    }

    /// <summary>
    /// Whether <paramref name="value"/> is within <paramref name="held"/>: nothing of it goes beyond what is held.
    /// </summary>
    internal bool Within(string? held, string? value, string? minimum, string? maximum)
    {
        if (this.winner != null)
        {
            if (value == null || value == minimum)
                return true;

            if (held == null)
                return false;

            if (this.order != null)
                return this.order(value, held) <= 0;

            return Normalize(this.winner(held, value)) == Normalize(held);
        }

        var heldEntries = Entries(held, minimum).ToList();

        return Entries(value, minimum).All(entry => Covered(heldEntries, entry, maximum));
    }

    /// <summary>
    /// The entries of <paramref name="value"/> that <paramref name="held"/> doesn't cover. Lists only.
    /// </summary>
    internal List<string> NotCovered(string? held, string? value, string? minimum, string? maximum)
    {
        var heldEntries = Entries(held, minimum).ToList();

        return Entries(value, minimum).Where(entry => !Covered(heldEntries, entry, maximum)).ToList();
    }

    /// <summary>
    /// Limits a submitted value to what the saver holds. A list value that is within it is returned as it is; one value
    /// is returned as its rule writes it (the winner of it and itself), as before 2.0.
    /// </summary>
    internal string? Reduce(string? value, string? held, string? minimum, string? maximum)
    {
        if (this.winner != null)
        {
            if (value == null)
                return minimum;

            var limit = held ?? minimum;

            if (limit == null)
                return minimum;

            //No one gives more than the action's maximum.
            if (maximum != null && !Within(maximum, limit, minimum, maximum))
                limit = maximum;

            var result = Within(limit, value, minimum, maximum) ? Normalize(value) : limit;

            //Nor less than its minimum.
            if (minimum != null && !Within(result, minimum, null, null))
                return minimum;

            return result;
        }

        if (value == null)
            return value;

        var heldEntries = Entries(held, minimum).ToList();
        var parts = value.Split(new[] { this.separator! }, StringSplitOptions.None);
        var kept = parts.Where(part => IsNothing(part, minimum) || Covered(heldEntries, part, maximum)).ToList();

        if (kept.Count == parts.Length)
            return value;

        var entries = kept.Where(part => !IsNothing(part, minimum)).ToList();

        return entries.Count == 0 ? minimum : string.Join(this.separator, entries);
    }

    /// <summary>
    /// On an update: what of the stored value the saver couldn't have given is kept. For one value, the stored value is
    /// kept as it is; for a list, the stored entries the saver doesn't cover are added to the result.
    /// </summary>
    internal string? Preserve(string? result, string? stored, string? held, string? minimum, string? maximum)
    {
        if (this.winner != null)
            return Within(held, stored, minimum, maximum) ? result : stored;

        var kept = NotCovered(held, stored, minimum, maximum);

        if (kept.Count == 0)
            return result;

        if (kept.Any(x => IsMaximum(x, maximum)))
            return maximum;

        var resultEntries = Entries(result, minimum).ToList();
        var added = kept.Where(x => !resultEntries.Contains(x, StringComparer.Ordinal)).Distinct(StringComparer.Ordinal).ToList();

        if (resultEntries.Count == 0)
            return string.Join(this.separator, added);

        if (added.Count == 0)
            return result;

        return result + this.separator + string.Join(this.separator, added);
    }

    //A value as the rule writes it: for example "020" as "20" with a numeric rule.
    private string? Normalize(string? value) => value == null ? null : this.winner!(value, value);

    private IEnumerable<string> Entries(string? value, string? minimum)
    {
        if (value == null)
            return Enumerable.Empty<string>();

        return value.Split(new[] { this.separator! }, StringSplitOptions.None).Where(x => !IsNothing(x, minimum));
    }

    private bool Covered(List<string> heldEntries, string entry, string? maximum)
    {
        return heldEntries.Any(held => IsMaximum(held, maximum) || this.covers!(held, entry));
    }

    private static bool IsNothing(string entry, string? minimum) => entry.Length == 0 || entry == minimum;

    private static bool IsMaximum(string entry, string? maximum) => maximum != null && entry == maximum;

    private static string? MaxOfDecimals(string? first, string? second)
    {
        if (first == null || second == null)
            return first == null && second == null ? null : decimal.Parse((first ?? second)!, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);

        var a = decimal.Parse(first, CultureInfo.InvariantCulture);
        var b = decimal.Parse(second, CultureInfo.InvariantCulture);

        if (a != b)
            return (a > b ? a : b).ToString(CultureInfo.InvariantCulture);

        //Equal values written differently (50 and 50.0): the shorter, so that either order gives the same text.
        var textA = a.ToString(CultureInfo.InvariantCulture);
        var textB = b.ToString(CultureInfo.InvariantCulture);

        return textA.Length != textB.Length ? (textA.Length < textB.Length ? textA : textB) : string.CompareOrdinal(textA, textB) <= 0 ? textA : textB;
    }

    private static int CompareDecimals(string first, string second)
    {
        return decimal.Parse(first, CultureInfo.InvariantCulture).CompareTo(decimal.Parse(second, CultureInfo.InvariantCulture));
    }
}

/// <summary>
/// Checks for hosts' own tests.
/// </summary>
public static class TextCombineAssert
{
    /// <summary>
    /// Combines <paramref name="values"/>, as one value per access tree, in every order, and throws when two orders give
    /// different values. A <see cref="TextCombine.Pick"/> rule that returns the last value is a last-assigned-wins rule
    /// under another name, which the compiler can't catch.
    /// </summary>
    public static void OrderIndependent(TextAction action, params string?[] values) => OrderIndependent(action.Combine, action.MinimumAccess, action.MaximumAccess, action.Name, values);

    /// <inheritdoc cref="OrderIndependent(TextAction, string?[])"/>
    public static void OrderIndependent(DynamicTextAction action, params string?[] values) => OrderIndependent(action.Combine, action.MinimumAccess, action.MaximumAccess, action.Name, values);

    private static void OrderIndependent(TextCombine combine, string? minimum, string? maximum, string? name, string?[] values)
    {
        if (values.Length > 8)
            throw new ArgumentException("Give at most 8 values; every order of them is tried.", nameof(values));

        var results = new Dictionary<string, string?>();

        foreach (var order in Orders(values.ToList()))
            results[string.Join(", ", order.Select(x => x ?? "null"))] = combine.Fold(order, minimum, maximum);

        var distinct = results.Values.Distinct().ToList();

        if (distinct.Count > 1)
            throw new InvalidOperationException(
                $"The combine rule of {name} depends on the order of the trees. " +
                string.Join("; ", results.Select(x => $"[{x.Key}] gives {x.Value ?? "null"}")));
    }

    private static IEnumerable<List<string?>> Orders(List<string?> items)
    {
        if (items.Count <= 1)
        {
            yield return items;
            yield break;
        }

        for (var i = 0; i < items.Count; i++)
        {
            var rest = items.Where((_, index) => index != i).ToList();

            foreach (var order in Orders(rest))
            {
                order.Insert(0, items[i]);
                yield return order;
            }
        }
    }
}
