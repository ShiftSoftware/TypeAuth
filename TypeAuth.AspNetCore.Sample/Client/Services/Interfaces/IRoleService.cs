using TypeAuth.AspNetCore.Sample.Shared.RoleDtos;

namespace TypeAuth.AspNetCore.Sample.Client.Services.Interfaces
{
    public interface IRoleService
    {
        Task<RoleDto> CrateRoleAsync(UpdateRoleDto role);
        Task<RoleDto> GetRoleAsync(int userId);
        Task<List<RoleDto>> GetRolesAsync();
        Task<bool> RemoveRoleAsync(int userId);
        Task<RoleDto> UpdateRoleAsync(int userId, UpdateRoleDto user);
    }
}