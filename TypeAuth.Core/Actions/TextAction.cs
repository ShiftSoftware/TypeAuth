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
        /// How the values of a user's trees are combined, and what a saver can give.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public TextCombine Combine { get; }

        /// <summary>
        /// The winner of a <see cref="TextCombine.Pick"/> rule, or null.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        [Obsolete("Use Combine. Comparer is derived from it and will be removed.")]
        public Func<string?, string?, string?>? Comparer => this.Combine.Winner;

        /// <summary>
        /// The union of a <see cref="TextCombine.List"/> rule, or null.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        [Obsolete("Use Combine. Merger is derived from it and will be removed.")]
        public Func<string?, string?, string?>? Merger => this.Combine.IsList ? (a, b) => this.Combine.Combine(a, b, this.MinimumAccess, this.MaximumAccess) : null;

        /// <summary>
        /// Removed in 2.0: pass a <see cref="TextCombine"/> as the second argument.
        /// </summary>
        [Obsolete(TextCombineMigration.Message, true)]
        public TextAction(string? name, string? description = null, string? minimumAccess = null, string? maximumAccess = null, Func<string?, string?, string?>? comparer = null, Func<string?, string?, string?>? merger = null)
            : base(name, ActionType.Text, description)
        {
            this.Combine = null!;
            throw new NotSupportedException(TextCombineMigration.Message);
        }

        /// <param name="name">Friendly display name.</param>
        /// <param name="combine">How the values of a user's trees are combined, and what a saver can give.</param>
        /// <param name="description">Optional description.</param>
        /// <param name="minimumAccess">The value representing no access.</param>
        /// <param name="maximumAccess">The value representing full access.</param>
        public TextAction(string? name, TextCombine combine, string? description = null, string? minimumAccess = null, string? maximumAccess = null)
            : base(name, ActionType.Text, description)
        {
            this.Combine = combine ?? throw new ArgumentNullException(nameof(combine));
            this.MinimumAccess = minimumAccess;
            this.MaximumAccess = maximumAccess;
        }
    }

    internal static class TextCombineMigration
    {
        internal const string Message = "A text action needs a TextCombine as its second argument: new TextAction(name, TextCombine.Pick(comparer), description, minimumAccess, maximumAccess), TextCombine.List(separator, covers) or TextCombine.UnionList(separator) in place of a merger.";
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
        /// How the values of a user's trees are combined, and what a saver can give. It applies to each row.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public TextCombine Combine { get; }

        /// <inheritdoc cref="TextAction.Comparer"/>
        [Newtonsoft.Json.JsonIgnore]
        [Obsolete("Use Combine. Comparer is derived from it and will be removed.")]
        public Func<string?, string?, string?>? Comparer => this.Combine.Winner;

        /// <inheritdoc cref="TextAction.Merger"/>
        [Newtonsoft.Json.JsonIgnore]
        [Obsolete("Use Combine. Merger is derived from it and will be removed.")]
        public Func<string?, string?, string?>? Merger => this.Combine.IsList ? (a, b) => this.Combine.Combine(a, b, this.MinimumAccess, this.MaximumAccess) : null;

        /// <summary>
        /// Removed in 2.0: pass a <see cref="TextCombine"/> as the second argument.
        /// </summary>
        [Obsolete(TextCombineMigration.Message, true)]
        public DynamicTextAction(string? name, string? description = null, string? minimumAccess = null, string? maximumAccess = null, Func<string?, string?, string?>? comparer = null, Func<string?, string?, string?>? merger = null)
            : base(name, ActionType.Text, description)
        {
            this.Combine = null!;
            throw new NotSupportedException(TextCombineMigration.Message);
        }

        /// <param name="name">Friendly display name.</param>
        /// <param name="combine">How the values of a user's trees are combined, and what a saver can give. It applies to each row.</param>
        /// <param name="description">Optional description.</param>
        /// <param name="minimumAccess">The value representing no access.</param>
        /// <param name="maximumAccess">The value representing full access.</param>
        public DynamicTextAction(string? name, TextCombine combine, string? description = null, string? minimumAccess = null, string? maximumAccess = null)
            : base(name, ActionType.Text, description)
        {
            this.Combine = combine ?? throw new ArgumentNullException(nameof(combine));
            this.MinimumAccess = minimumAccess;
            this.MaximumAccess = maximumAccess;
        }
    }
}
