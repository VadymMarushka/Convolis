using Convolis.Api.Services.Abstractions;

namespace Convolis.Api.Services.Implementations
{
    /// <summary>
    /// A singleton service tracking real-time user presence.
    /// Handles scenarios where a single user might have multiple active connections (e.g., multiple browser tabs or devices).
    /// </summary>
    public class OnlineTrackerService : IOnlineTrackerService
    {
        // Maps UserId to the number of active SignalR connections to properly handle multiple tabs.
        private readonly Dictionary<Guid, int> _onlineUsers = new();

        public void UserConnected(Guid userId)
        {
            // SignalR hubs are multi-threaded; locking ensures thread safety for the shared dictionary.
            lock (_onlineUsers)
            {
                if (!_onlineUsers.ContainsKey(userId)) _onlineUsers[userId] = 0;
                _onlineUsers[userId]++;
            }
        }

        public void UserDisconnected(Guid userId)
        {
            lock (_onlineUsers)
            {
                if (!_onlineUsers.ContainsKey(userId)) return;
                _onlineUsers[userId]--;

                // Only remove the user from online presence if all their tabs/connections are closed
                if (_onlineUsers[userId] <= 0) _onlineUsers.Remove(userId);
            }
        }

        public bool IsUserOnline(Guid userId) => _onlineUsers.ContainsKey(userId);

        public int GetOnlineCount(IEnumerable<Guid> userIds)
            => userIds.Count(id => _onlineUsers.ContainsKey(id));

        public int TotalOnlineGlobal() => _onlineUsers.Count;
    }
}
