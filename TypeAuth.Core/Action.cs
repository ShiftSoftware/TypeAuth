namespace ShiftSoftware.TypeAuth.Core
{

    /// <summary>
    /// Action is the smallest unit that can be used in the TypeAuth Access Control System
    /// </summary>
    public class Action : Attribute
    {
        /// <summary>
        /// Friendly name for identifying the Action.
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// Not all actions are the same. They could be a bool, Read/Write combo, or a more complicated data structure represented as a String.
        /// </summary>
        public ActionType Type { get; set; }
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
        /// Additional description about the Action
        /// </summary>
        public string? Description { get; set; }

        public Action()
        {

        }

        public Action(string? name, ActionType actionType, string? description = null, string? maximumAccess = null, string? minimumAccess = null)
        {
            this.Name = name;
            this.Type = actionType;
            this.MaximumAccess = maximumAccess;
            this.MinimumAccess = minimumAccess;
            this.Description= description;
        }
    }
}
