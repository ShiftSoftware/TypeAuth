using TypeAuth.AspNetCore.Sample.Client.Services.Interfaces;
using TypeAuth.AspNetCore.Sample.Shared.RoleDtos;
using TypeAuth.AspNetCore.Sample.Shared.UserDtos;

namespace TypeAuth.AspNetCore.Sample.Client.Services
{
    public class RoleService : IRoleService
    {
        private readonly IHttpService http;
        private readonly string url = "api/roles";

        public RoleService(IHttpService http)
        {
            this.http = http;
        }

        public async Task<List<RoleDto>> GetRolesAsync()
        {
            return await http.GetAsync<List<RoleDto>>(url);
        }

        public async Task<RoleDto> CrateRoleAsync(UpdateRoleDto role)
        {
            return await http.PostAsync<RoleDto, UpdateRoleDto>(url, role);
        }

        public async Task<bool> RemoveRoleAsync(int userId)
        {
            return await http.DeleteAsync($"{url}/{userId}");
        }

        public async Task<RoleDto> UpdateRoleAsync(int userId, UpdateRoleDto user)
        {
            return await http.PutAsync<RoleDto, UpdateRoleDto>($"{url}/{userId}", user);
        }

        public async Task<RoleDto> GetRoleAsync(int userId)
        {
            return await http.GetAsync<RoleDto>($"{url}/{userId}");
        }
    }
}
