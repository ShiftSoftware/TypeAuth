﻿namespace ShiftSoftware.TypeAuth.Core.Actions
{
    public class ReadAction : Action
    {
        public ReadAction()
        {

        }

        public ReadAction(string? name, string? description): base(name, ActionType.Read, description)
        {

        }
    }
}
