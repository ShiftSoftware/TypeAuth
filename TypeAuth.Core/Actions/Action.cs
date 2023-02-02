namespace ShiftSoftware.TypeAuth.Core.Actions
{
    public class ActionBase
    {
        /// <summary>
        /// Friendly name for identifying the Action.
        /// </summary>
        public string? Name { get; set; }

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

    public class DynamicAction : ActionBase
    {
        /// <summary>
        /// The unique identifier for the data item (or Row). This is useful for Dynamic Actions
        /// </summary>
        public string? Id { get; set; }

        public DynamicAction() : base() { }

        public DynamicAction(ActionType actionType) : base (actionType)
        {
        }

        public DynamicAction(string? name, ActionType actionType, string? description = null) : base(name, actionType, description)
        {

        }
    }
}
