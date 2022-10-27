using TypeAuth.AspNetCore.Sample.Server.Models;
using TypeAuth.AspNetCore.Sample.Shared.UserDtos;

namespace TypeAuth.AspNetCore.Sample.Server.Repos.Interfaces
{
    public interface IUserRepo
    {
        Task<User> CreateUserAsync(User user);
        public User RemoveUser(User user);
        Task<List<UserDto>> GetUsersAsync();
        Task<User> GetUserAsync(int id);
        Task<User> GetUserByUserNameAsync(string username);
    }
}