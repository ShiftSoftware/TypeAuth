using TypeAuth.AspNetCore.Sample.Server.Models;
using TypeAuth.AspNetCore.Sample.Server.Repos.Interfaces;
using TypeAuth.AspNetCore.Sample.Server.Services.Interfaces;
using TypeAuth.AspNetCore.Sample.Shared.UserDtos;

namespace TypeAuth.AspNetCore.Sample.Server.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepo userRepo;
        private readonly IHashService hashService;

        public UserService(IUserRepo userRepo, IHashService hashService)
        {
            this.userRepo = userRepo;
            this.hashService = hashService;
        }

        public async Task<User> RegisterUserAsync(RegisterUserDto registerUserDto)
        {
            var hash = hashService.GenerateHash(registerUserDto.Password);

            User user = new User
            {
                PasswordHash = hash.PasswordHash,
                Salt = hash.Salt,
                Username = registerUserDto.Username
            };

            return await userRepo.CreateUserAsync(user);
        }

        public async Task<List<UserDto>> GetUsersAsync()
        {
            return await userRepo.GetUsersAsync();
        }

        public async Task<User> GetUserAsync(int id)
        {
            return await userRepo.GetUserAsync(id);
        }

        public User UpdateUserAsync(User user, RegisterUserDto registerUserDto)
        {
            var hash = hashService.GenerateHash(registerUserDto.Password);

            user.PasswordHash = hash.PasswordHash;
            user.Salt = hash.Salt;
            user.Username = registerUserDto.Username;

            return user;
        }

        public User RemoveUser(User user)
        {
            return userRepo.RemoveUser(user);
        }
    }
}
