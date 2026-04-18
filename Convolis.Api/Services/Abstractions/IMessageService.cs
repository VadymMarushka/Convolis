using Convolis.Shared.DTOs;

namespace Convolis.Api.Services.Abstractions
{
    public interface IMessageService
    {
        Task<MessageDTO?> CreateMessageAsync(Guid senderId, Guid conversationId, string content);
    }
}
