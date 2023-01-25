using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace ShiftSoftware.TypeAuth.Blazor.Providers;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly HttpClient http;
    private readonly TypeAuthBlazorOptions options;
    private readonly ITokenProvider tokenProvider;

    public CustomAuthStateProvider(
        HttpClient http, 
        TypeAuthBlazorOptions options, 
        ITokenProvider tokenProvider)
    {
        this.http = http;
        this.options = options;
        this.tokenProvider = tokenProvider;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        List<Claim> claims = new List<Claim>();

        var token = tokenProvider.GetToken();

        var identity = new ClaimsIdentity();
        if (token is not null)
        {
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
            claims = ParseClaimsFromJwt(token).ToList();
            identity = new ClaimsIdentity(claims, "jwt");
        }

        var user = new ClaimsPrincipal(identity);
        var state = new AuthenticationState(user);

        NotifyAuthenticationStateChanged(Task.FromResult(state));

        return state;
    }

    private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var payload = jwt.Split('.')[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
        return keyValuePairs!.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString()!));
    }

    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }
}
