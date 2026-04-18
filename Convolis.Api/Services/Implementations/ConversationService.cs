using Convolis.Api.Data;
using Convolis.Api.Data.Entities;
using Convolis.Api.Services.Abstractions;
using Convolis.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Convolis.Api.Services.Implementations
{
    public class ConversationService(ConvolisDbContext context, IOnlineTrackerService onlineTracker) : IConversationService
    {
        public async Task<ConversationDTO?> CreateChatByUsernameAsync(Guid currentUserId, string targetUsername)
        {
            var targetUser = await context.Users
                .FirstOrDefaultAsync(u => u.Username == targetUsername);

            if (targetUser == null || targetUser.Id == currentUserId)
                return null;

            var conversation = new Conversation
            {
                Id = Guid.NewGuid(),
                Name = $""
            };

            var participants = new List<Participant>
            {
                new Participant { UserId = currentUserId, ConversationId = conversation.Id },
                new Participant { UserId = targetUser.Id, ConversationId = conversation.Id }
            };

            context.Conversations.Add(conversation);
            context.Participants.AddRange(participants);

            await context.SaveChangesAsync();

            return new ConversationDTO
            {
                Id = conversation.Id,
                Name = targetUsername
            };
        }
        public async Task<List<ConversationDTO>> GetUserConversationsAsync(Guid userId)
        {
            var userConversations = await context.Participants
                .Where(p => p.UserId == userId)
                .Select(p => new {
                    Conversation = p.Conversation,
                    AllParticipantIds = p.Conversation.Participants.Select(x => x.UserId).ToList()
                })
                .ToListAsync();

            return userConversations.Select(x => new ConversationDTO
            {
                Id = x.Conversation.Id,
                Name = x.Conversation.Id == ConvolisDbContext.GlobalChatId
                       ? "Глобальний чат"
                       : x.Conversation.Name,
                ParticipantsCount = x.AllParticipantIds.Count,
                OnlineCount = x.Conversation.Id == ConvolisDbContext.GlobalChatId
                              ? onlineTracker.TotalOnlineGlobal()
                              : onlineTracker.GetOnlineCount(x.AllParticipantIds),
            }).ToList();
        }

        public async Task<ConversationDetailsDTO?> GetConversationByIdAsync(Guid conversationId)
        {
            var conversation = await context.Conversations
                .Include(c => c.Messages)
                .ThenInclude(m => m.Sender)
                .FirstOrDefaultAsync(c => c.Id == conversationId);

            if (conversation == null) return null;

            return new ConversationDetailsDTO
            {
                Id = conversation.Id,
                Name = conversation.Name,
                Messages = conversation.Messages
                    .OrderBy(m => m.Timestamp)
                    .Select(m => new MessageDTO
                    {
                        Content = m.Content,
                        SenderName = m.Sender.Username,
                        SenderId = m.SenderId,
                        ConversationId = m.ConversationId,
                        Sentiment = m.Sentiment,
                        Timestamp = m.Timestamp
                    }).ToList()
            };
        }
    }
}
