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

        [Newtonsoft.Json.JsonIgnore]
        public Func<string?, string?, string?>? Comparer { get; set; }

        public TextAction()
        {

        }

        public TextAction(string? name, string? description = null, string? minimumAccess = null, string? maximumAccess = null, Func<string?, string?, string?>? comparer = null) 
            : base(name, ActionType.Text, description)
        {
            this.MinimumAccess = minimumAccess;
            this.MaximumAccess = maximumAccess;
            this.Comparer = comparer;
        }
    }
}
