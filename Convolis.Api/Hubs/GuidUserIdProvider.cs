using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Convolis.Api.Hubs
{
    /// <summary>
    /// Custom user ID provider for SignalR.
    /// Extracts the user's unique identifier from the JWT claims to enable targeted user-specific messaging.
    /// </summary>
    public class GuidUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            // Retrieves the user ID stored in the NameIdentifier claim during JWT token generation
            return connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
