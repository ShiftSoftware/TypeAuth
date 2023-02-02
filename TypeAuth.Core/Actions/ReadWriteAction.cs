namespace ShiftSoftware.TypeAuth.Core.Actions
{
    public class ReadWriteAction : Action
    {
        public ReadWriteAction() : base(ActionType.ReadWrite)
        {
        }

        public ReadWriteAction(string? name, string? description = null) : base(name, ActionType.ReadWrite, description)
        {
        }
    }

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
