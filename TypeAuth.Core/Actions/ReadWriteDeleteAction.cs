namespace ShiftSoftware.TypeAuth.Core.Actions
{
    /// <summary>
    /// An action that supports Read, Write, and Delete access levels.
    /// </summary>
    public class ReadWriteDeleteAction : Action
    {
        public ReadWriteDeleteAction() : base(ActionType.ReadWriteDelete)
        {
        }

        public ReadWriteDeleteAction(string? name, string? description = null) : base(name, ActionType.ReadWriteDelete, description)
        {
        }
    }

    /// <summary>
    /// A dynamic (per-ID) action that supports Read, Write, and Delete access levels.
    /// </summary>
    public class DynamicReadWriteDeleteAction : DynamicAction
    {
        public DynamicReadWriteDeleteAction() : base(ActionType.ReadWriteDelete)
        {
        }

        public DynamicReadWriteDeleteAction(string? name, string? description = null) : base(name, ActionType.ReadWriteDelete, description)
        {
        }
    }
}
