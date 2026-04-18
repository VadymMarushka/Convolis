using Azure.AI.TextAnalytics;
using Convolis.Api.Data;
using Convolis.Api.Data.Entities;
using Convolis.Api.Services.Abstractions;
using Convolis.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Convolis.Api.Services.Implementations
{
    public class MessageService(ConvolisDbContext context, TextAnalyticsClient aiClient) : IMessageService
    {
        public async Task<MessageDTO?> CreateMessageAsync(Guid senderId, Guid conversationId, string content)
        {
            var isParticipant = await context.Participants
                .AnyAsync(p => p.ConversationId == conversationId && p.UserId == senderId);

            if (!isParticipant) return null;

            var sender = await context.Users.FindAsync(senderId);
            if (sender == null) return null;

            string messageSentiment = "Neutral";

            if (!string.IsNullOrWhiteSpace(content))
            {
                try
                {
                    DocumentSentiment documentSentiment = await aiClient.AnalyzeSentimentAsync(content);
                    messageSentiment = documentSentiment.Sentiment.ToString();
                }
                catch (Exception)
                {
                    // Якщо, щось з AI-йкою піде не так - то буде просто Neutral
                }
            }

            var message = new Message
            {
                Id = Guid.NewGuid(),
                Content = content,
                SenderId = senderId,
                ConversationId = conversationId,
                Timestamp = DateTime.UtcNow,
                Sentiment = messageSentiment
            };

            context.Messages.Add(message);
            await context.SaveChangesAsync();

            return new MessageDTO
            {
                Content = message.Content,
                SenderName = sender.Username,
                SenderId = message.SenderId,
                ConversationId = message.ConversationId,
                Timestamp = message.Timestamp,
                Sentiment = message.Sentiment
            };
        }
    }
}