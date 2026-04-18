using Convolis.Api.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Convolis.Api.Hubs
{
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
                foreach (var chat in userChats)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, chat.Id.ToString());
                }

                // Notify all chats that this user is now online
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

                // Fetch chats AFTER decrement so OnlineCount is already updated
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

        // Called by ConversationController after creating a new 1-on-1 chat
        // Adds the target user to the new group and notifies them
        public async Task NotifyNewConversation(Guid targetUserId, Guid conversationId)
        {
            // Find all active connections of the target user and add them to the new group
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