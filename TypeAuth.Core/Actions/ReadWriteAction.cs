namespace ShiftSoftware.TypeAuth.Core.Actions
{
    public class ReadWriteAction : Action
    {
        public ReadWriteAction()
        {
        }

        public ReadWriteAction(string? name, string? description) : base(name, ActionType.ReadWrite, description)
        {
        }
    }
}
