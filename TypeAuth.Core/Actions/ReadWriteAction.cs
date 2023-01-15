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
}
