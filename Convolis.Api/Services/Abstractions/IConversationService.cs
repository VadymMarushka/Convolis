using Convolis.Shared.DTOs;

namespace Convolis.Api.Services.Abstractions
{
    public interface IConversationService
    {
        Task<(ConversationDTO? conversation, Guid? targetUserId)> CreateChatByUsernameAsync(Guid currentUserId, string targetUsername);
        Task<List<ConversationDTO>> GetUserConversationsAsync(Guid userId);
        Task<ConversationDetailsDTO?> GetConversationByIdAsync(Guid conversationId, Guid? sender);
    }
}