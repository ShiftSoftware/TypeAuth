using TypeAuth.AspNetCore.Sample.Server.Models;
using TypeAuth.AspNetCore.Sample.Shared.RoleDtos;

namespace TypeAuth.AspNetCore.Sample.Server.Repos.Interfaces
{
    public interface IRoleRepo
    {
        Task<Role> CrateRoleAsync(Role role);
        Task<List<RoleDto>> GetRolesAsync();
        Task<Role> GetRoleAsync(int id);
        Role RemoveRole(Role role);
    }
}