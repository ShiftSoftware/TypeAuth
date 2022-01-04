namespace ShiftSoftware.TypeAuth.Core.Actions
{
    public class ReadWriteAction : Action
    {
        public ReadWriteAction()
        {
        }

        public ReadWriteAction(string? name, string? description = null) : base(name, ActionType.ReadWrite, description)
        {
        }
    }
}
