namespace ShiftSoftware.TypeAuth.Core.Actions
{
    public class ReadWriteDeleteAction : Action
    {
        public ReadWriteDeleteAction() : base(ActionType.ReadWriteDelete)
        {
        }

        public ReadWriteDeleteAction(string? name, string? description = null) : base(name, ActionType.ReadWriteDelete, description)
        {
        }
    }
    
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
