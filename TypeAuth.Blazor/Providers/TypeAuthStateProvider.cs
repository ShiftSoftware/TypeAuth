using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace ShiftSoftware.TypeAuth.Blazor.Providers;

public class TypeAuthStateProvider : AuthenticationStateProvider
{
    private readonly TypeAuthBlazorOptions options;
    private readonly ITokenProvider tokenProvider;

    public TypeAuthStateProvider(
        TypeAuthBlazorOptions options, 
        ITokenProvider tokenProvider)
    {
        this.options = options;
        this.tokenProvider = tokenProvider;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = tokenProvider.GetToken();

        var identity = new ClaimsIdentity();
        if (token is not null)
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token);
            var tokenS = jsonToken as JwtSecurityToken;

            identity = new ClaimsIdentity(tokenS?.Claims, "jwt");
        }

        var user = new ClaimsPrincipal(identity);
        var state = new AuthenticationState(user);

        NotifyAuthenticationStateChanged(Task.FromResult(state));

        return state;
    }
}
