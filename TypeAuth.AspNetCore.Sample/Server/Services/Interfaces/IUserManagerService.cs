using TypeAuth.AspNetCore.Sample.Shared.UserManagerDtos;

namespace TypeAuth.AspNetCore.Sample.Server.Services.Interfaces
{
    public interface IUserManagerService
    {
        Task<string> LoginAsync(LoginDto login);
    }
}