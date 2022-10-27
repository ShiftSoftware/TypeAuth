using TypeAuth.AspNetCore.Sample.Server.General;

namespace TypeAuth.AspNetCore.Sample.Server.Services.Interfaces
{
    public interface IHashService
    {
        HashModel GenerateHash(string password);
        bool VerifyPassword(string password, byte[] salt, byte[] passwordHash);
    }
}