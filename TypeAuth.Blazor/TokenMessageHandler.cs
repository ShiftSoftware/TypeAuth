using ShiftSoftware.TypeAuth.Blazor.Providers;
using System.Net.Http.Headers;

namespace ShiftSoftware.TypeAuth.Blazor;

public class TokenMessageHandler : DelegatingHandler
{
    private readonly ITokenProvider tokenProvider;

    public TokenMessageHandler(ITokenProvider tokenProvider)
    {
        this.tokenProvider = tokenProvider;
    }

    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var tokrn = tokenProvider.GetToken();
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokrn);

        return base.Send(request, cancellationToken);
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var tokrn = tokenProvider.GetToken();
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokrn);

        return base.SendAsync(request, cancellationToken);
    }
}
