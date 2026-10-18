using System.Globalization;

namespace ShiftSoftware.TypeAuth.Core.Actions;

/// <summary>
/// A <see cref="TextAction"/> specialized for decimal values, combined with <see cref="TextCombine.MaxDecimal"/>.
/// </summary>
public class DecimalAction : TextAction
{
    /// <param name="name">Friendly display name.</param>
    /// <param name="description">Optional description.</param>
    /// <param name="minimumAccess">The decimal value representing no access.</param>
    /// <param name="maximumAccess">The decimal value representing full access.</param>
    public DecimalAction(string? name, string? description = null, decimal? minimumAccess = null, decimal? maximumAccess = null) : base(
            name,
            TextCombine.MaxDecimal,
            description,
            minimumAccess?.ToString(CultureInfo.InvariantCulture),
            maximumAccess?.ToString(CultureInfo.InvariantCulture)
        )
    {

    }
}

/// <summary>
/// A <see cref="DynamicTextAction"/> specialized for decimal values, combined with <see cref="TextCombine.MaxDecimal"/>.
/// </summary>
public class DynamicDecimalAction : DynamicTextAction
{
    /// <param name="name">Friendly display name.</param>
    /// <param name="description">Optional description.</param>
    /// <param name="minimumAccess">The decimal value representing no access.</param>
    /// <param name="maximumAccess">The decimal value representing full access.</param>
    public DynamicDecimalAction(string? name, string? description = null, decimal? minimumAccess = null, decimal? maximumAccess = null) : base(
            name,
            TextCombine.MaxDecimal,
            description,
            minimumAccess?.ToString(CultureInfo.InvariantCulture),
            maximumAccess?.ToString(CultureInfo.InvariantCulture)
        )
    {

    }
}
