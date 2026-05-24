namespace ShiftSoftware.TypeAuth.Core.Actions
{
    /// <summary>
    /// An action with a custom string-based access value, bounded by minimum and maximum values.
    /// </summary>
    public class TextAction : Action, ITextAccessProperties
    {
        /// <summary>
        /// For non-standard Action Types the Maximum (Or Full Access) should be specified.
        /// Example: When defining an Action for Discount Percentage. The MaximumAcess is 100.
        /// This is especially important for determining the Access of a child Action when it's Parent Action Tree is Granted.
        /// </summary>
        public string? MaximumAccess { get; set; }
        /// <summary>
        /// For non-standard Action Types the Minimum (Or No Access) should be specified.
        /// Example: When defining an Action for Discount Percentage. The MinimumAcess is 0.
        /// </summary>
        public string? MinimumAccess { get; set; }

        /// <summary>
        /// Compares two access values and returns the winning value. Mutually exclusive with <see cref="Merger"/>.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public Func<string?, string?, string?>? Comparer { get; set; }
        /// <summary>
        /// Merges two access values into a combined value. Mutually exclusive with <see cref="Comparer"/>.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public Func<string?, string?, string?>? Merger { get; set; }

        public TextAction() : base(ActionType.Text)
        {

        }

        /// <param name="name">Friendly display name.</param>
        /// <param name="description">Optional description.</param>
        /// <param name="minimumAccess">The value representing no access.</param>
        /// <param name="maximumAccess">The value representing full access.</param>
        /// <param name="comparer">Function that compares two values and returns the winner. Cannot be combined with <paramref name="merger"/>.</param>
        /// <param name="merger">Function that merges two values into one. Cannot be combined with <paramref name="comparer"/>.</param>
        public TextAction(string? name, string? description = null, string? minimumAccess = null, string? maximumAccess = null, Func<string?, string?, string?>? comparer = null, Func<string?, string?, string?>? merger = null)
            : base(name, ActionType.Text, description)
        {
            this.MinimumAccess = minimumAccess;
            this.MaximumAccess = maximumAccess;
            this.Comparer = comparer;
            this.Merger = merger;

            if (this.Merger != null && this.Comparer != null)
                throw new Exception("Comparer and Merger can not be specified for the same action. Only one is allowed at a time.");
        }
    }

    /// <summary>
    /// A dynamic (per-ID) action with a custom string-based access value, bounded by minimum and maximum values.
    /// </summary>
    public class DynamicTextAction : DynamicAction, ITextAccessProperties
    {
        /// <summary>
        /// For non-standard Action Types the Maximum (Or Full Access) should be specified.
        /// Example: When defining an Action for Discount Percentage. The MaximumAcess is 100.
        /// This is especially important for determining the Access of a child Action when it's Parent Action Tree is Granted.
        /// </summary>
        public string? MaximumAccess { get; set; }
        /// <summary>
        /// For non-standard Action Types the Minimum (Or No Access) should be specified.
        /// Example: When defining an Action for Discount Percentage. The MinimumAcess is 0.
        /// </summary>
        public string? MinimumAccess { get; set; }

        /// <summary>
        /// Compares two access values and returns the winning value. Mutually exclusive with <see cref="Merger"/>.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public Func<string?, string?, string?>? Comparer { get; set; }
        /// <summary>
        /// Merges two access values into a combined value. Mutually exclusive with <see cref="Comparer"/>.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public Func<string?, string?, string?>? Merger { get; set; }

        public DynamicTextAction() : base(ActionType.Text)
        {

        }

        /// <param name="name">Friendly display name.</param>
        /// <param name="description">Optional description.</param>
        /// <param name="minimumAccess">The value representing no access.</param>
        /// <param name="maximumAccess">The value representing full access.</param>
        /// <param name="comparer">Function that compares two values and returns the winner. Cannot be combined with <paramref name="merger"/>.</param>
        /// <param name="merger">Function that merges two values into one. Cannot be combined with <paramref name="comparer"/>.</param>
        public DynamicTextAction(string? name, string? description = null, string? minimumAccess = null, string? maximumAccess = null, Func<string?, string?, string?>? comparer = null, Func<string?, string?, string?>? merger = null)
            : base(name, ActionType.Text, description)
        {
            this.MinimumAccess = minimumAccess;
            this.MaximumAccess = maximumAccess;
            this.Comparer = comparer;
            this.Merger = merger;

            if (this.Merger != null && this.Comparer != null)
                throw new Exception("Comparer and Merger can not be specified for the same action. Only one is allowed at a time.");
        }
    }
}
