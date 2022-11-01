using TypeAuth.AspNetCore.Sample.Client.Services.Interfaces;
using TypeAuth.AspNetCore.Sample.Shared.UserManagerDtos;

namespace TypeAuth.AspNetCore.Sample.Client.Services
{
    public class UserManagerService : IUserManagerService
    {
        private readonly IHttpService http;
        private readonly string url = "api/UserManager";

        public UserManagerService(IHttpService http)
        {
            this.http = http;
        }

        public async Task<string> LoginAsync(LoginDto login)
        {
            return await http.PostAsync<LoginDto>($"{url}/Login", login);
        }
    }
}
