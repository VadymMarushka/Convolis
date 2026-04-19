namespace Convolis.Api.Services.Abstractions
{
    /// <summary>
    /// Defines the contract for tracking real-time user online presence.
    /// Used in conjunction with SignalR hubs to monitor active connections.
    /// </summary>
    public interface IOnlineTrackerService
    {
        /// <summary>
        /// Calculates how many users from a specified list are currently online.
        /// Useful for determining active participants in a specific 1-on-1 or group conversation.
        /// </summary>
        /// <param name="userIds">A collection of user IDs to check.</param>
        /// <returns>The number of currently online users from the provided list.</returns>
        int GetOnlineCount(IEnumerable<Guid> userIds);

        /// <summary>
        /// Determines whether a specific user has at least one active connection.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>True if the user is online; otherwise, false.</returns>
        bool IsUserOnline(Guid userId);

        /// <summary>
        /// Retrieves the total number of all currently connected users across the entire application.
        /// Primarily used to display the active user count in the Global Chat.
        /// </summary>
        /// <returns>The total count of online users.</returns>
        int TotalOnlineGlobal();

        /// <summary>
        /// Registers a user's connection. Should be called when a user successfully connects to the SignalR hub.
        /// </summary>
        /// <param name="userId">The unique identifier of the connecting user.</param>
        void UserConnected(Guid userId);

        /// <summary>
        /// Registers a user's disconnection. Should be called when a user disconnects from the SignalR hub.
        /// </summary>
        /// <param name="userId">The unique identifier of the disconnecting user.</param>
        void UserDisconnected(Guid userId);
    }
}