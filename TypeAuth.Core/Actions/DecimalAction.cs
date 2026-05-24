using System.Globalization;

namespace ShiftSoftware.TypeAuth.Core.Actions;

/// <summary>
/// A <see cref="TextAction"/> specialized for decimal values, with a built-in max comparer.
/// </summary>
public class DecimalAction : TextAction
{
    /// <param name="name">Friendly display name.</param>
    /// <param name="description">Optional description.</param>
    /// <param name="minimumAccess">The decimal value representing no access.</param>
    /// <param name="maximumAccess">The decimal value representing full access.</param>
    public DecimalAction(string? name, string? description = null, decimal? minimumAccess = null, decimal? maximumAccess = null) : base(
            name,
            description,
            minimumAccess?.ToString(CultureInfo.InvariantCulture),
            maximumAccess?.ToString(CultureInfo.InvariantCulture),
            (a, b) =>
            {
                var numbers = new List<decimal>();

                if (a != null)
                    numbers.Add(decimal.Parse(a, CultureInfo.InvariantCulture));
                if (b != null)
                    numbers.Add(decimal.Parse(b, CultureInfo.InvariantCulture));

                if (numbers.Count > 0)
                    return numbers.Max().ToString(CultureInfo.InvariantCulture);

                return null;
            }
        )
    {

    }
}

/// <summary>
/// A <see cref="DynamicTextAction"/> specialized for decimal values, with a built-in max comparer.
/// </summary>
public class DynamicDecimalAction : DynamicTextAction
{
    /// <param name="name">Friendly display name.</param>
    /// <param name="description">Optional description.</param>
    /// <param name="minimumAccess">The decimal value representing no access.</param>
    /// <param name="maximumAccess">The decimal value representing full access.</param>
    public DynamicDecimalAction(string? name, string? description = null, decimal? minimumAccess = null, decimal? maximumAccess = null) : base(
            name,
            description,
            minimumAccess?.ToString(CultureInfo.InvariantCulture),
            maximumAccess?.ToString(CultureInfo.InvariantCulture),
            (a, b) =>
            {
                var numbers = new List<decimal>();

                if (a != null)
                    numbers.Add(decimal.Parse(a, CultureInfo.InvariantCulture));
                if (b != null)
                    numbers.Add(decimal.Parse(b, CultureInfo.InvariantCulture));

                if (numbers.Count > 0)
                    return numbers.Max().ToString(CultureInfo.InvariantCulture);

                return null;
            }
        )
    {

    }
}
