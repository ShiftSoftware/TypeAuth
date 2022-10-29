using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using TypeAuth.AspNetCore.Sample.Server.Data;
using TypeAuth.AspNetCore.Sample.Server.Models;
using TypeAuth.AspNetCore.Sample.Server.Repos.Interfaces;
using TypeAuth.AspNetCore.Sample.Shared.UserDtos;

namespace TypeAuth.AspNetCore.Sample.Server.Repos
{
    public class UserRepo : IUserRepo
    {
        private readonly TypeAuthDbContext db;
        private readonly IMapper mapper;

        public UserRepo(TypeAuthDbContext db, IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        public async Task<User> CreateUserAsync(User user)
        {
            await db.Users.AddAsync(user);

            return user;
        }

        public User RemoveUser(User user)
        {
            db.Users.Remove(user);
            return user;
        }

        public async Task<List<UserDto>> GetUsersAsync()
        {
            return await db.Users.AsNoTracking()
                .ProjectTo<UserDto>(mapper.ConfigurationProvider).ToListAsync();
        }

        public async Task<User> GetUserAsync(int id)
        {
            return await db.Users.Include(x => x.UserInRoles).ThenInclude(x => x.Role).
                SingleOrDefaultAsync(x => x.Id == id);
        }

        public async Task<User> GetUserByUserNameAsync(string username)
        {
            return await db.Users.Include(x => x.UserInRoles).ThenInclude(x => x.Role).
                SingleOrDefaultAsync(x => x.Username.ToLower() == username.ToLower());
        }
    }
}
