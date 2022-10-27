namespace TypeAuth.AspNetCore.Sample.Server.General
{
    public class HashModel
    {
        public byte[] Salt { get; set; }

        public byte[] PasswordHash { get; set; }
    }
}
