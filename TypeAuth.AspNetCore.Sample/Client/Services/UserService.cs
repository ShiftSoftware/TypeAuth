using TypeAuth.AspNetCore.Sample.Client.Services.Interfaces;
using TypeAuth.AspNetCore.Sample.Shared.UserDtos;

namespace TypeAuth.AspNetCore.Sample.Client.Services
{
    public class UserService : IUserService
    {
        private readonly IHttpService http;
        private readonly string url = "api/users";

        public UserService(IHttpService http)
        {
            this.http = http;
        }

        public async Task<List<UserDto>> GetUsersAsync()
        {
            return await http.GetAsync<List<UserDto>>(url);
        }

        public async Task<UserDto> CrateUserAsync(RegisterUserDto user)
        {
            return await http.PostAsync<UserDto, RegisterUserDto>(url, user);
        }

        public async Task<bool> RemoveUserAsync(int userId)
        {
            return await http.DeleteAsync($"{url}/{userId}");
        }

        public async Task<UserDto> UpdateUserAsync(int userId,RegisterUserDto user)
        {
            return await http.PutAsync<UserDto, RegisterUserDto>($"{url}/{userId}", user);
        }

        public async Task<UserDto> GetUserAsync(int userId) 
        {
            return await http.GetAsync<UserDto>($"{url}/{userId}");
        }
    }
}
