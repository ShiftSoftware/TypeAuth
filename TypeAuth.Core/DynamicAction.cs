using Action = ShiftSoftware.TypeAuth.Core.Actions.Action;
namespace ShiftSoftware.TypeAuth.Core
{
    /// <summary>
    /// An action that's dynamically generated from data. This is used for handling Data Level (Or Row-Level) Access Control.
    /// </summary>
    public class DynamicAction : Action
    {
        /// <summary>
        /// The unique identifier for the data item (or Row).
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Constructs an action from data.
        /// </summary>
        /// <param name="id">The unique identifier for the data item (or Row)</param>
        /// <param name="name">Friendly name for identifying the Action.</param>
        /// <param name="actionType">Sets the ActionType of this Dynamic Action.</param>
        /// <param name="description">Additional description about the Action.</param>
        public DynamicAction(string id, string? name, ActionType actionType, string? description = null)
        {
            this.Id = id;
            this.Name = name;
            this.Type = actionType;
            this.Description = description;
        }
    }
}
