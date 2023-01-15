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
}
