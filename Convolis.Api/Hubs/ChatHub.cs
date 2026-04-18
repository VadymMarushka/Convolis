using Convolis.Api.Services.Abstractions;
using Convolis.Api.Services.Implementations;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Convolis.Api.Hubs
{
    public class ChatHub(IConversationService conversationService, IOnlineTrackerService onlineTracker) : Hub
    {
        private static readonly Dictionary<Guid, int> OnlineUsers = new();
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

                    await Clients.Group(chat.Id.ToString()).SendAsync("UpdateConversationStatus", new
                    {
                        ConversationId = chat.Id,
                        OnlineCount = chat.OnlineCount
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
                lock (OnlineUsers)
                {
                    OnlineUsers[userId.Value]--;
                    if (OnlineUsers[userId.Value] <= 0) OnlineUsers.Remove(userId.Value);
                }
                await Clients.Group("Global").SendAsync("UpdateOnlineCount", OnlineUsers.Count);
            }
            await base.OnDisconnectedAsync(exception);
        }
        private Guid? GetUserId()
        {
            var claim = Context.User?.FindFirst(ClaimTypes.NameIdentifier);
            return Guid.TryParse(claim?.Value, out var id) ? id : null;
        }
    }
}
