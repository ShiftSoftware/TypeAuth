using ShiftSoftware.TypeAuth.Blazor.Providers;
using System.Net.Http.Headers;

namespace ShiftSoftware.TypeAuth.Blazor;

public class TokenMessageHandler : DelegatingHandler
{
    private readonly ITokenProvider tokenProvider;

    public TokenMessageHandler(ITokenProvider tokenProvider)
    {
        //add this to solve "The inner handler has not been assigned"
        InnerHandler = new HttpClientHandler();

        this.tokenProvider = tokenProvider;
    }

    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = tokenProvider.GetToken();
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return base.Send(request, cancellationToken);
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = tokenProvider.GetToken();
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return base.SendAsync(request, cancellationToken);
    }
}
