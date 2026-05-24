namespace ShiftSoftware.TypeAuth.Core.Actions
{
    /// <summary>
    /// An action that supports Read and Write access levels.
    /// </summary>
    public class ReadWriteAction : Action
    {
        public ReadWriteAction() : base(ActionType.ReadWrite)
        {
        }

        public ReadWriteAction(string? name, string? description = null) : base(name, ActionType.ReadWrite, description)
        {
        }
    }

    /// <summary>
    /// A dynamic (per-ID) action that supports Read and Write access levels.
    /// </summary>
    public class DynamicReadWriteAction : DynamicAction
    {
        public DynamicReadWriteAction() : base(ActionType.ReadWrite)
        {
        }

        public DynamicReadWriteAction(string? name, string? description = null) : base(name, ActionType.ReadWrite, description)
        {
        }
    }
}
