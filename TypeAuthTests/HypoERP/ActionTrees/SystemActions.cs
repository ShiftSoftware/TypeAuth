using ShiftSoftware.TypeAuth.Core;

namespace TypeAuthTests.HypoERP.ActionTrees
{
    [ActionTree("System Actions", "Actions related to the System Module and Admistration.")]
    public class SystemActions
    {
        [ActionTree("Login", "")]
        public class Login
        {
            public static readonly Action MultipleSession = new Action("Multiple Login Sessions", ActionType.Boolean, "Ability to have multiple sessions. Or Be logged in on multiple browsers/devices at once.");
            public static readonly Action DestroyOtherSession = new Action("Destroy Other Sessions", ActionType.Boolean, "Ability to destroy other login sessions. Or Logout from other browsers/devices when trying to login on a new browser/device.");
        }

        [ActionTree("Users", "Actions Related to the Users Module")]
        public class UserModule
        {
            public static readonly Action Users = new Action("User Access", ActionType.ReadWriteDelete);
            public static readonly Action SetOrResetPassword = new Action("Set or Reset Passwords", ActionType.Boolean, "Ability to Set or Reset Users' Passwords.");
            public static readonly Action DestroyLoginSessions = new Action("Destroy Login Sessions", ActionType.Boolean, "Ability to force users to logout from browsers/devices they're arleady logged in.");
            public static readonly Action Roles = new Action("Role Access", ActionType.ReadWriteDelete);
        }
    }
}
