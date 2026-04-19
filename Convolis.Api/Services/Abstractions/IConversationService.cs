using Convolis.Shared.DTOs;

namespace Convolis.Api.Services.Abstractions
{
    /// <summary>
    /// Defines the contract for managing chat conversations, including 1-on-1 chats and the Global Chat.
    /// </summary>
    public interface IConversationService
    {
        /// <summary>
        /// Attempts to create a new 1-on-1 conversation with a specified user by their username.
        /// </summary>
        /// <param name="currentUserId">The unique identifier of the user initiating the chat.</param>
        /// <param name="targetUsername">The username of the user to start a conversation with.</param>
        /// <returns>A tuple containing the created conversation DTO and the target user's ID, or null if the target user is not found or invalid.</returns>
        Task<(ConversationDTO? conversation, Guid? targetUserId)> CreateChatByUsernameAsync(Guid currentUserId, string targetUsername);

        /// <summary>
        /// Retrieves a list of all conversations a specific user is participating in. 
        /// Automatically resolves dynamic chat names and active online participant counts.
        /// </summary>
        /// <param name="userId">The unique identifier of the user whose conversations are being retrieved.</param>
        /// <returns>A list of conversation summaries.</returns>
        Task<List<ConversationDTO>> GetUserConversationsAsync(Guid userId);

        /// <summary>
        /// Retrieves the full details of a specific conversation, including its message history.
        /// </summary>
        /// <param name="conversationId">The unique identifier of the conversation to retrieve.</param>
        /// <param name="sender">The ID of the user requesting the details, used to dynamically resolve the chat's display name.</param>
        /// <returns>The detailed conversation object including messages, or null if not found.</returns>
        Task<ConversationDetailsDTO?> GetConversationByIdAsync(Guid conversationId, Guid? sender);
    }
}