namespace TypeAuth.AspNetCore.Sample.Client.Models
{
    public class UserInRoleModel
    {
        public string RoleName { get; set; }
        public int RoleId { get; set; }
        public bool Granted { get; set; }
    }
}
