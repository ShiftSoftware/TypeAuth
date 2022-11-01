using TypeAuth.AspNetCore.Sample.Server.Models;

namespace TypeAuth.AspNetCore.Sample.Server.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateJwtToken(User user, int expireAfterDays = 30);
    }
}