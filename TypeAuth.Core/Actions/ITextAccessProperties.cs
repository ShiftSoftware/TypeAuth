namespace ShiftSoftware.TypeAuth.Core.Actions
{
    /// <summary>
    /// Shared contract for text-based action properties.
    /// Implemented by both <see cref="TextAction"/> and <see cref="DynamicTextAction"/>
    /// to eliminate duplication of <c>MaximumAccess</c>, <c>MinimumAccess</c> and <c>Combine</c>.
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
        /// How the values of a user's trees are combined, and what a saver can give.
        /// </summary>
        TextCombine Combine { get; }
    }
}
