namespace Convolis.Api.Services.Abstractions
{
    public interface IOnlineTrackerService
    {
        int GetOnlineCount(IEnumerable<Guid> userIds);
        bool IsUserOnline(Guid userId);
        int TotalOnlineGlobal();
        void UserConnected(Guid userId);
        void UserDisconnected(Guid userId);
    }
}