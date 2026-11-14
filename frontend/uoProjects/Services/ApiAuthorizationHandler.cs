using Microsoft.AspNetCore.Components.WebAssembly.Http;
using System.Net.Http.Headers;
using uoProjects.Auth;
using uoProjects.Exceptions;

namespace uoProjects.Services;

public sealed class ApiAuthorizationHandler(Uri apiAddress, AuthSession auth) : DelegatingHandler(new HttpClientHandler())
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.RequestUri is not { } uri || !apiAddress.IsBaseOf(uri))
        {
            throw new InvalidOperationException("The API client can only send requests to the configured API.");
        }

        var user = await auth.GetUserAsync()
            ?? throw ServiceException.FromResponse("Authenticate", System.Net.HttpStatusCode.Unauthorized, null);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", user.AccessToken);
        request.SetBrowserRequestOption("redirect", "error");
        return await base.SendAsync(request, cancellationToken);
    }
}
