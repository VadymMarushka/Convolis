using Convolis.UI.Services;
using System.Net.Http.Headers;

namespace Convolis.UI.Http
{
    public class AuthenticatedHttpHandler : DelegatingHandler
    {
        private readonly AuthStateService _authState;

        public AuthenticatedHttpHandler(AuthStateService authState)
        {
            _authState = authState;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(_authState.AccessToken))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", _authState.AccessToken);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}