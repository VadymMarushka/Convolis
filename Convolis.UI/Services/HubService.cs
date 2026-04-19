using Convolis.Shared.DTOs;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Convolis.UI.Services
{
    /// <summary>
    /// Manages the real-time SignalR connection, handling incoming messages, 
    /// status updates, and conversation notifications.
    /// </summary>
    public class HubService(AuthStateService authState, IConfiguration config) : IAsyncDisposable
    {
        private HubConnection? _connection;
        private bool _isStarting;

        public event Action<MessageDTO>? OnMessageReceived;
        public event Action<Guid, int, int>? OnConversationStatusUpdated;
        public event Action<Guid>? OnConversationCreated;
        public event Action? OnReconnected;

        public bool IsConnected => _connection?.State == HubConnectionState.Connected;

        /// <summary>
        /// Initializes and starts the SignalR connection if it's not already active.
        /// </summary>
        public async Task StartAsync()
        {
            if (_connection != null || _isStarting) return;

            _isStarting = true;
            try
            {
                var baseUrl = config["ApiBaseUrl"] ?? "https://localhost:5001";

                _connection = new HubConnectionBuilder()
                    .WithUrl($"{baseUrl}/chathub", options =>
                    {
                        // Dynamically provide the latest token for every connection attempt
                        options.AccessTokenProvider = () => Task.FromResult(authState.AccessToken);
                    })
                    .WithAutomaticReconnect()
                    .Build();

                // Handler: New message received
                _connection.On<MessageDTO>("ReceiveMessage", msg => OnMessageReceived?.Invoke(msg));

                // Handler: Conversation online status or participant count changed
                _connection.On<JsonElement>("UpdateConversationStatus", json =>
                {
                    try
                    {
                        var convId = json.GetProperty("conversationId").GetGuid();
                        var online = json.GetProperty("onlineCount").GetInt32();
                        var members = json.TryGetProperty("participantsCount", out var mc) ? mc.GetInt32() : -1;

                        OnConversationStatusUpdated?.Invoke(convId, online, members);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error parsing status update: {ex.Message}");
                    }
                });

                // Handler: Another user created a conversation with us
                _connection.On<Guid>("ConversationCreated", convId => OnConversationCreated?.Invoke(convId));

                // Handle automatic reconnection logic
                _connection.Reconnected += _ =>
                {
                    OnReconnected?.Invoke();
                    return Task.CompletedTask;
                };

                await _connection.StartAsync();
                Console.WriteLine("SignalR: Connected.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SignalR Connection Error: {ex.Message}");
            }
            finally
            {
                _isStarting = false;
            }
        }

        /// <summary>
        /// Restarts the hub connection. Useful for forcing the user into new SignalR groups 
        /// (e.g., after joining a new conversation).
        /// </summary>
        public async Task RestartAsync()
        {
            Console.WriteLine("SignalR: Restarting...");
            await StopAsync();
            await StartAsync();
        }

        /// <summary>
        /// Safely stops and disposes of the current connection.
        /// </summary>
        public async Task StopAsync()
        {
            if (_connection != null)
            {
                await _connection.StopAsync();
                await _connection.DisposeAsync();
                _connection = null;
            }
        }

        public async ValueTask DisposeAsync() => await StopAsync();
    }
}