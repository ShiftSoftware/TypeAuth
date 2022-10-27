namespace TypeAuth.AspNetCore.Sample.Server.Models
{
    public class Role
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string AccessTree { get; set; }

        public List<UserInRole> UserInRoles { get; set; }

        public Role()
        {
            UserInRoles = new List<UserInRole>();
        }
    }
}
