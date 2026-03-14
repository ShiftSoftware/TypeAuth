namespace ShiftSoftware.TypeAuth.Core.Actions
{
    /// <summary>
    /// Shared contract for text-based action properties.
    /// Implemented by both <see cref="TextAction"/> and <see cref="DynamicTextAction"/>
    /// to eliminate duplication of <c>MaximumAccess</c>, <c>MinimumAccess</c>, <c>Comparer</c>, and <c>Merger</c>.
    /// </summary>
    internal interface ITextAccessProperties
    {
        /// <summary>
        /// For non-standard Action Types the Maximum (Or Full Access) should be specified.
        /// Example: When defining an Action for Discount Percentage. The MaximumAccess is 100.
        /// This is especially important for determining the Access of a child Action when it's Parent Action Tree is Granted.
        /// </summary>
        string? MaximumAccess { get; set; }

        /// <summary>
        /// For non-standard Action Types the Minimum (Or No Access) should be specified.
        /// Example: When defining an Action for Discount Percentage. The MinimumAccess is 0.
        /// </summary>
        string? MinimumAccess { get; set; }

        /// <summary>
        /// Compares two access values and returns the winning value.
        /// Mutually exclusive with <see cref="Merger"/>.
        /// </summary>
        Func<string?, string?, string?>? Comparer { get; set; }

        /// <summary>
        /// Merges two access values into a combined value.
        /// Mutually exclusive with <see cref="Comparer"/>.
        /// </summary>
        Func<string?, string?, string?>? Merger { get; set; }
    }
}
