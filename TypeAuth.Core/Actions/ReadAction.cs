namespace ShiftSoftware.TypeAuth.Core.Actions
{
    /// <summary>
    /// An action that supports only Read access.
    /// </summary>
    public class ReadAction : Action
    {
        public ReadAction() : base(ActionType.Read)
        {

        }

        public ReadAction(string? name, string? description = null) : base(name, ActionType.Read, description)
        {

        }
    }

    /// <summary>
    /// A dynamic (per-ID) action that supports only Read access.
    /// </summary>
    public class DynamicReadAction : DynamicAction
    {
        public DynamicReadAction() : base(ActionType.Read)
        {

        }

        public DynamicReadAction(string? name, string? description = null) : base(name, ActionType.Read, description)
        {

        }
    }
}
