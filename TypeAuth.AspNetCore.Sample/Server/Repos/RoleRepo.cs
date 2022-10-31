using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using TypeAuth.AspNetCore.Sample.Server.ActionTreeModels;
using TypeAuth.AspNetCore.Sample.Server.Data;
using TypeAuth.AspNetCore.Sample.Server.Models;
using TypeAuth.AspNetCore.Sample.Server.Repos.Interfaces;
using TypeAuth.AspNetCore.Sample.Shared.RoleDtos;

namespace TypeAuth.AspNetCore.Sample.Server.Repos
{
    public class RoleRepo : IRoleRepo
    {
        private readonly TypeAuthDbContext db;
        private readonly IMapper mapper;

        public RoleRepo(TypeAuthDbContext db, IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        public async Task<Role> CrateRoleAsync(Role role)
        {
            await db.Roles.AddAsync(role);
            return role;
        }

        public async Task<List<RoleDto>> GetRolesAsync()
        {
            return (await db.Roles.AsNoTracking().ProjectTo<RoleModel>(mapper.ConfigurationProvider).ToListAsync())
                .AsQueryable().ProjectTo<RoleDto>(mapper.ConfigurationProvider).ToList();
        }

        public async Task<Role> GetRoleAsync(int id)
        {
            return await db.Roles.FindAsync(id);
        }

        public Role RemoveRole(Role role)
        {
            db.Roles.Remove(role);
            return role;
        }
    }
}
