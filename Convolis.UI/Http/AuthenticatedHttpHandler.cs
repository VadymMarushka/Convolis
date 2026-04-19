using Convolis.UI.Services;
using System.Net.Http.Headers;

namespace Convolis.UI.Http
{
    /// <summary>
    /// A custom HTTP message handler that automatically attaches the JWT Bearer token 
    /// to outgoing requests targeting secured API endpoints.
    /// </summary>
    public class AuthenticatedHttpHandler(AuthStateService authState) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Attach the access token to the Authorization header if the user is authenticated
            if (!string.IsNullOrEmpty(authState.AccessToken))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", authState.AccessToken);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}