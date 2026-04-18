using Convolis.Shared.DTOs;
using System.Net.Http.Json;

namespace Convolis.UI.Services
{
    public class ChatService
    {
        private readonly HttpClient _http;

        public ChatService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<ConversationDTO>> GetConversationsAsync()
        {
            try
            {
                return await _http.GetFromJsonAsync<List<ConversationDTO>>("api/conversations")
                       ?? new List<ConversationDTO>();
            }
            catch
            {
                return new List<ConversationDTO>();
            }
        }

        public async Task<ConversationDetailsDTO?> GetConversationAsync(Guid id)
        {
            try
            {
                return await _http.GetFromJsonAsync<ConversationDetailsDTO>($"api/conversations/{id}");
            }
            catch
            {
                return null;
            }
        }

        public async Task<ConversationDTO?> CreateConversationAsync(string targetUsername)
        {
            try
            {
                var response = await _http.PostAsync(
                    $"api/conversations?targetUsername={Uri.EscapeDataString(targetUsername)}", null);

                if (!response.IsSuccessStatusCode) return null;
                return await response.Content.ReadFromJsonAsync<ConversationDTO>();
            }
            catch
            {
                return null;
            }
        }

        public async Task<MessageDTO?> SendMessageAsync(Guid conversationId, string text)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/message", new SendMessageRequestDTO
                {
                    ConversationId = conversationId,
                    Text = text
                });

                if (!response.IsSuccessStatusCode) return null;
                return await response.Content.ReadFromJsonAsync<MessageDTO>();
            }
            catch
            {
                return null;
            }
        }
    }
}
