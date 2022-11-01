using TypeAuth.AspNetCore.Sample.Shared.UserDtos;

namespace TypeAuth.AspNetCore.Sample.Client.Services.Interfaces
{
    public interface IUserService
    {
        Task<List<UserDto>> GetUsersAsync();
        Task<UserDto> CrateUserAsync(RegisterUserDto user);
        Task<bool> RemoveUserAsync(int userId);
        Task<UserDto> UpdateUserAsync(int userId, RegisterUserDto user);
        Task<UserDto> GetUserAsync(int userId);
        Task<UserDto> SetUserInRoleAsync(int userId, int[] roleIds);
    }
}