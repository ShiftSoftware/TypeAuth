using TypeAuth.AspNetCore.Sample.Server.Repos.Interfaces;
using TypeAuth.AspNetCore.Sample.Server.Services.Interfaces;
using TypeAuth.AspNetCore.Sample.Shared.UserManagerDtos;

namespace TypeAuth.AspNetCore.Sample.Server.Services
{
    public class UserManagerService : IUserManagerService
    {
        private readonly IUserRepo userRepo;
        private readonly ITokenService tokenService;
        private readonly IHashService hashService;

        public UserManagerService(IUserRepo userRepo, ITokenService tokenService, IHashService hashService)
        {
            this.userRepo = userRepo;
            this.tokenService = tokenService;
            this.hashService = hashService;
        }

        public async Task<string> LoginAsync(LoginDto login)
        {
            var user = await userRepo.GetUserByUserNameAsync(login.Username);

            if (user is null)
                return null;

            if (!hashService.VerifyPassword(login.Password, user.Salt, user.PasswordHash))
                return null;

            return tokenService.GenerateJwtToken(user);
        }
    }
}
