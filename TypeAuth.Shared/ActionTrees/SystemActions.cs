using ShiftSoftware.TypeAuth.Core;
using ShiftSoftware.TypeAuth.Core.Actions;

namespace ShiftSoftware.TypeAuth.Shared.ActionTrees
{
    [ActionTree("System Actions", "Actions related to the System Module and Admistration.")]
    public class SystemActions
    {
        [ActionTree("Login", "")]
        public class Login
        {
            public BooleanAction MultipleSession = new BooleanAction("Multiple Login Sessions", "Ability to have multiple sessions. Or Be logged in on multiple browsers/devices at once.");
            public BooleanAction DestroyOtherSession = new BooleanAction("Destroy Other Sessions", "Ability to destroy other login sessions. Or Logout from other browsers/devices when trying to login on a new browser/device.");
        }

        [ActionTree("Users", "Actions Related to the Users Module")]
        public class UserModule
        {
            public ReadWriteDeleteAction Users = new ReadWriteDeleteAction("User Access");
            public BooleanAction SetOrResetPassword = new BooleanAction("Set or Reset Passwords", "Ability to Set or Reset Users' Passwords.");
            public BooleanAction DestroyLoginSessions = new BooleanAction("Destroy Login Sessions", "Ability to force users to logout from browsers/devices they're arleady logged in.");
            public ReadWriteDeleteAction Roles = new ReadWriteDeleteAction("Role Access");
        }
    }
}
