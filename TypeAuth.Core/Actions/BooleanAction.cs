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

    public class DynamicBooleanAction : DynamicAction
    {
        public DynamicBooleanAction() : base(ActionType.Boolean)
        {

        }

        public DynamicBooleanAction(string? name, string? description = null) : base(name, ActionType.Boolean, description)
        {

        }
    }
}
