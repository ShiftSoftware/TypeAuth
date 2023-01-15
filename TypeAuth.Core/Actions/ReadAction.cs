namespace ShiftSoftware.TypeAuth.Core.Actions
{
    public class ReadAction : Action
    {
        public ReadAction() : base(ActionType.Read)
        {

        }

        public ReadAction(string? name, string? description = null): base(name, ActionType.Read, description)
        {

        }
    }
}
