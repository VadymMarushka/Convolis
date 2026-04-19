using Convolis.Api.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Convolis.Api.Hubs
{
    /// <summary>
    /// SignalR hub managing real-time bidirectional communication.
    /// Handles connection lifecycles, user presence, and group-based event broadcasting.
    /// </summary>
    [Authorize]
    public class ChatHub(
        IConversationService conversationService,
        IOnlineTrackerService onlineTracker) : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = GetUserId();
            if (userId.HasValue)
            {
                onlineTracker.UserConnected(userId.Value);

                var userChats = await conversationService.GetUserConversationsAsync(userId.Value);

                // Assign the connection to SignalR groups corresponding to the user's active conversations
                foreach (var chat in userChats)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, chat.Id.ToString());
                }

                // Broadcast real-time presence updates to all participants in these conversations
                foreach (var chat in userChats)
                {
                    await Clients.Group(chat.Id.ToString()).SendAsync("UpdateConversationStatus", new
                    {
                        ConversationId = chat.Id,
                        OnlineCount = chat.OnlineCount,
                        ParticipantsCount = chat.ParticipantsCount
                    });
                }
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetUserId();
            if (userId.HasValue)
            {
                onlineTracker.UserDisconnected(userId.Value);

                // Fetch conversations AFTER the tracker decrement to broadcast the accurately updated online count
                var userChats = await conversationService.GetUserConversationsAsync(userId.Value);
                foreach (var chat in userChats)
                {
                    await Clients.Group(chat.Id.ToString()).SendAsync("UpdateConversationStatus", new
                    {
                        ConversationId = chat.Id,
                        OnlineCount = chat.OnlineCount,
                        ParticipantsCount = chat.ParticipantsCount
                    });
                }
            }
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Invoked externally (e.g., from controllers) to notify a target user about a newly created 1-on-1 chat.
        /// </summary>
        public async Task NotifyNewConversation(Guid targetUserId, Guid conversationId)
        {
            // Sends an event to all active connections (tabs/devices) belonging to the target user
            await Clients.User(targetUserId.ToString())
                .SendAsync("ConversationCreated", conversationId);
        }

        private Guid? GetUserId()
        {
            var claim = Context.User?.FindFirst(ClaimTypes.NameIdentifier);
            return Guid.TryParse(claim?.Value, out var id) ? id : null;
        }
    }
}