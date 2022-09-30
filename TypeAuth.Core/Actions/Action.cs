namespace ShiftSoftware.TypeAuth.Core.Actions
{

    /// <summary>
    /// Action is the smallest unit that can be used in the TypeAuth Access Control System
    /// </summary>
    public  class Action
    {
        /// <summary>
        /// The unique identifier for the data item (or Row). This is useful for Dynamic Actions
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public string Id { get; set; }

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

        public Dictionary<string, T> Dynamic<T>(Func<Dictionary<string, T>> function) where T : Action
        {
            return function.Invoke();
        }
    }
}
