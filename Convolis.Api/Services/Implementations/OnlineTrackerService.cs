using Convolis.Api.Services.Abstractions;

namespace Convolis.Api.Services.Implementations
{
    public class OnlineTrackerService : IOnlineTrackerService
    {
        private readonly Dictionary<Guid, int> _onlineUsers = new();

        public void UserConnected(Guid userId)
        {
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
                if (_onlineUsers[userId] <= 0) _onlineUsers.Remove(userId);
            }
        }

        public bool IsUserOnline(Guid userId) => _onlineUsers.ContainsKey(userId);

        public int GetOnlineCount(IEnumerable<Guid> userIds)
            => userIds.Count(id => _onlineUsers.ContainsKey(id));

        public int TotalOnlineGlobal() => _onlineUsers.Count;
    }
}
