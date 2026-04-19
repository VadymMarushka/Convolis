using Convolis.Shared.DTOs;

namespace Convolis.Api.Services.Abstractions
{
    /// <summary>
    /// Defines the contract for managing chat messages, including real-time sentiment analysis integration.
    /// </summary>
    public interface IMessageService
    {
        /// <summary>
        /// Creates and persists a new message within a specific conversation. 
        /// Automatically evaluates the sentiment of the message content using Azure Cognitive Services.
        /// </summary>
        /// <param name="senderId">The unique identifier of the user sending the message.</param>
        /// <param name="conversationId">The unique identifier of the target conversation.</param>
        /// <param name="content">The text content of the message.</param>
        /// <returns>The created message DTO containing the analyzed sentiment, or null if the user is not a participant of the conversation.</returns>
        Task<MessageDTO?> CreateMessageAsync(Guid senderId, Guid conversationId, string content);
    }
}
