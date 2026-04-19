using Convolis.Api.Data;
using Convolis.Api.Data.Entities;
using Convolis.Api.Services.Abstractions;
using Convolis.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Convolis.Api.Services.Implementations
{
    /// <summary>
    /// Manages chat conversations, including private 1-on-1 chats and the Global Chat.
    /// Handles dynamic resolution of chat names and online participant counts.
    /// </summary>
    public class ConversationService(ConvolisDbContext context, IOnlineTrackerService onlineTracker) : IConversationService
    {
        public async Task<(ConversationDTO? conversation, Guid? targetUserId)> CreateChatByUsernameAsync(
            Guid currentUserId, string targetUsername)
        {
            var targetUser = await context.Users
                .FirstOrDefaultAsync(u => u.Username == targetUsername);

            // Prevent creating a chat with a non-existent user or with oneself
            if (targetUser == null || targetUser.Id == currentUserId)
                return (null, null);

            var conversation = new Conversation
            {
                Id = Guid.NewGuid(),
                Name = targetUsername
            };

            context.Conversations.Add(conversation);
            context.Participants.AddRange(new[]
            {
                new Participant { UserId = currentUserId, ConversationId = conversation.Id },
                new Participant { UserId = targetUser.Id, ConversationId = conversation.Id }
            });

            await context.SaveChangesAsync();

            return (new ConversationDTO
            {
                Id = conversation.Id,
                Name = targetUsername
            }, targetUser.Id);
        }

        public async Task<List<ConversationDTO>> GetUserConversationsAsync(Guid userId)
        {
            var userConversations = await context.Participants
                .Where(p => p.UserId == userId)
                .Select(p => new {
                    Conversation = p.Conversation,
                    AllParticipants = p.Conversation.Participants
                        .Select(x => new { x.UserId, x.User.Username })
                        .ToList(),
                    LastMessage = p.Conversation.Messages
                        .OrderByDescending(m => m.Timestamp)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return userConversations.Select(x => {
                var isGlobal = x.Conversation.Id == ConvolisDbContext.GlobalChatId;
                var allParticipantIds = x.AllParticipants.Select(p => p.UserId).ToList();

                // Dynamically resolve the chat name:
                // Global Chat has a fixed name, while 1-on-1 chats display the other participant's username.
                string name;
                if (isGlobal)
                    name = "Global Chat 🌍";
                else
                {
                    var other = x.AllParticipants.FirstOrDefault(p => p.UserId != userId);
                    name = other?.Username ?? x.Conversation.Name;
                }

                return new ConversationDTO
                {
                    Id = x.Conversation.Id,
                    Name = name,
                    ParticipantsCount = allParticipantIds.Count,
                    // Calculate online users specifically for the Global Chat vs private chats
                    OnlineCount = isGlobal
                        ? onlineTracker.TotalOnlineGlobal()
                        : onlineTracker.GetOnlineCount(allParticipantIds),
                    LastMessageContent = x.LastMessage?.Content,
                    LastMessageTimestamp = x.LastMessage?.Timestamp,
                    LastMessageSentiment = x.LastMessage?.Sentiment,
                };
            }).ToList();
        }

        public async Task<ConversationDetailsDTO?> GetConversationByIdAsync(Guid conversationId, Guid? sender)
        {
            // Eagerly load messages and participants to map them to the DTO
            var conversation = await context.Conversations
                .Include(c => c.Messages)
                .ThenInclude(m => m.Sender)
                .Include(c => c.Participants)
                .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(c => c.Id == conversationId);

            if (conversation == null) return null;

            string name;
            if (conversationId == ConvolisDbContext.GlobalChatId)
                name = "Global Chat 🌍";
            else if (sender is null || sender == Guid.Empty)
                name = "Chat";
            else
            {
                var other = conversation.Participants.FirstOrDefault(p => p.UserId != sender);
                name = other?.User.Username ?? "Chat";
            }

            return new ConversationDetailsDTO
            {
                Id = conversation.Id,
                Name = name,
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