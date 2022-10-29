using TypeAuth.AspNetCore.Sample.Server.Models;
using TypeAuth.AspNetCore.Sample.Shared.UserDtos;

namespace TypeAuth.AspNetCore.Sample.Server.Services.Interfaces
{
    public interface IUserService
    {
        Task<User> RegisterUserAsync(RegisterUserDto registerUserDto);
        Task<List<UserDto>> GetUsersAsync();
        Task<User> GetUserAsync(int id);
        User UpdateUserAsync(User user, RegisterUserDto registerUserDto);
        User RemoveUser(User user);
    }
}