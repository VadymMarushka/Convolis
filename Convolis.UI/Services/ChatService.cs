using Convolis.Shared.DTOs;
using System.Net.Http.Json;

namespace Convolis.UI.Services
{
    /// <summary>
    /// Service responsible for managing chat conversations and messages via the API.
    /// </summary>
    public class ChatService(HttpClient http)
    {
        /// <summary>
        /// Retrieves a list of all conversations the current user is participating in.
        /// </summary>
        public async Task<List<ConversationDTO>> GetConversationsAsync()
        {
            try
            {
                return await http.GetFromJsonAsync<List<ConversationDTO>>("api/conversations")
                       ?? new List<ConversationDTO>();
            }
            catch (Exception ex)
            {
                // In a production app, consider using a proper ILogger
                Console.WriteLine($"Error fetching conversations: {ex.Message}");
                return new List<ConversationDTO>();
            }
        }

        /// <summary>
        /// Fetches full details of a specific conversation, including its message history.
        /// </summary>
        public async Task<ConversationDetailsDTO?> GetConversationAsync(Guid id)
        {
            try
            {
                return await http.GetFromJsonAsync<ConversationDetailsDTO>($"api/conversations/{id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching conversation {id}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Initiates a new 1-on-1 conversation with another user.
        /// </summary>
        /// <param name="targetUsername">The username of the person to start a chat with.</param>
        public async Task<ConversationDTO?> CreateConversationAsync(string targetUsername)
        {
            try
            {
                // Using query parameter with encoding to handle special characters in usernames
                var response = await http.PostAsync(
                    $"api/conversations?targetUsername={Uri.EscapeDataString(targetUsername)}", null);

                if (!response.IsSuccessStatusCode) return null;
                return await response.Content.ReadFromJsonAsync<ConversationDTO>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating conversation with {targetUsername}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Sends a text message to a specific conversation.
        /// </summary>
        public async Task<MessageDTO?> SendMessageAsync(Guid conversationId, string text)
        {
            try
            {
                var response = await http.PostAsJsonAsync("api/message", new SendMessageRequestDTO
                {
                    ConversationId = conversationId,
                    Text = text
                });

                if (!response.IsSuccessStatusCode) return null;
                return await response.Content.ReadFromJsonAsync<MessageDTO>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
                return null;
            }
        }
    }
}