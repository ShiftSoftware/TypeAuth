namespace ShiftSoftware.TypeAuth.Core.Actions
{
    public class ReadAction : Action
    {
        public ReadAction()
        {

        }

        public ReadAction(string? name, string? description = null): base(name, ActionType.Read, description)
        {

        }
    }
}
