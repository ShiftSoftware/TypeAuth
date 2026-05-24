namespace ShiftSoftware.TypeAuth.Core.Actions
{
    /// <summary>
    /// Base class for all action types in the TypeAuth access control system.
    /// </summary>
    public class ActionBase
    {
        /// <summary>
        /// Friendly name for identifying the Action.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Dot-delimited path that uniquely identifies this action within the action tree hierarchy.
        /// </summary>
        public string? Path { get; set; }

        /// <summary>
        /// Additional description about the Action
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Not all actions are the same. They could be a bool, Read/Write combo, or a more complicated data structure represented as a String.
        /// </summary>
        public ActionType Type { get; set; }

        public ActionBase() { }

        public ActionBase(ActionType actionType)
        {
            this.Type = actionType;
        }

        public ActionBase(string? name, ActionType actionType, string? description = null)
        {
            this.Name = name;
            this.Description = description;
            this.Type = actionType;
        }
    }

    /// <summary>
    /// Action is the smallest unit that can be used in the TypeAuth Access Control System
    /// </summary>
    public class Action : ActionBase
    {
        public Action() { }

        public Action(ActionType actionType) : base(actionType)
        {
        }

        public Action(string? name, ActionType actionType, string? description = null) : base(name, actionType, description)
        {
        }
    }

    /// <summary>
    /// An action that supports instance-level (per-ID) access control for individual data items.
    /// </summary>
    public class DynamicAction : ActionBase
    {
        /// <summary>
        /// The unique identifier for the data item (or Row). This is useful for Dynamic Actions
        /// </summary>
        public string? Id { get; set; }

        public DynamicAction() : base()
        {
            Items = new();
        }

        public DynamicAction(ActionType actionType) : base(actionType)
        {
            this.Items = new();
        }

        public DynamicAction(string? name, ActionType actionType, string? description = null) : base(name, actionType, description)
        {
            this.Items = new();
        }

        internal List<KeyValuePair<string, string>> Items { get; set; }

        /// <summary>
        /// Populates the expandable items for this dynamic action. Each item represents a data entity that can be individually granted or denied access.
        /// </summary>
        /// <param name="items">Key-value pairs where the key is the item ID and the value is its display name.</param>
        /// <param name="addSelf">If true, adds a "Self" entry that resolves to the current user's own ID at evaluation time.</param>
        /// <param name="addEmptyOrNull">If true, adds an "Unassigned" entry for items with no assigned owner.</param>
        public void Expand(List<KeyValuePair<string, string>> items, bool addSelf = false, bool addEmptyOrNull = false)
        {
            this.Items = items.ToList();


            if (addEmptyOrNull)
                this.Items.Insert(0, new KeyValuePair<string, string>(TypeAuthContext.EmptyOrNullKey, "Unassigned"));

            if (addSelf)
                this.Items.Insert(0, new KeyValuePair<string, string>(TypeAuthContext.SelfReferenceKey, "Self"));
        }
    }
}
