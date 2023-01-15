namespace ShiftSoftware.TypeAuth.Core.Actions
{
    public class BooleanAction : Action
    {
        public BooleanAction() : base(ActionType.Boolean)
        {

        }

        public BooleanAction(string? name, string? description = null) : base(name, ActionType.Boolean, description)
        {

        }
    }
}
