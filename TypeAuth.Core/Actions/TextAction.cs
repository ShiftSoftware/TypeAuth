namespace ShiftSoftware.TypeAuth.Core.Actions
{
    public class TextAction : Action
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

        public Func<string?, string?, string?>? SortFunction { get; set; }

        public TextAction()
        {

        }

        public TextAction(string? name, string? description = null, string? maximumAccess = null, string? minimumAccess = null, Func<string?, string?, string?>? winner = null) 
            : base(name, ActionType.Text, description)
        {
            this.MaximumAccess = maximumAccess;
            this.MinimumAccess = minimumAccess;
            this.SortFunction = winner;
        }
    }
}
