namespace ShiftSoftware.TypeAuth.Core.Actions
{

    /// <summary>
    /// Action is the smallest unit that can be used in the TypeAuth Access Control System
    /// </summary>
    public  class Action
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

        public Action()
        {

        }

        public Action(string? name, ActionType actionType, string? description = null)
        {
            this.Name = name;
            this.Description = description;
            this.Type = actionType;
        }
    }
}
