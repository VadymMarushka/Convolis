using Convolis.Shared.DTOs;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;

namespace Convolis.UI.Services
{
    public class HubService : IAsyncDisposable
    {
        private readonly AuthStateService _authState;
        private readonly IConfiguration _config;
        private HubConnection? _connection;

        public event Action<MessageDTO>? OnMessageReceived;
        public event Action<Guid, int, int>? OnConversationStatusUpdated;
        public event Action<Guid>? OnConversationCreated;
        public event Action? OnReconnected;

        public bool IsConnected => _connection?.State == HubConnectionState.Connected;

        public HubService(AuthStateService authState, IConfiguration config)
        {
            _authState = authState;
            _config = config;
        }

        public async Task StartAsync()
        {
            if (_connection != null) return;

            var baseUrl = _config["ApiBaseUrl"]!;

            _connection = new HubConnectionBuilder()
                .WithUrl($"{baseUrl}/chathub", options =>
                {
                    options.AccessTokenProvider = () =>
                        Task.FromResult(_authState.AccessToken);
                })
                .WithAutomaticReconnect()
                .Build();

            _connection.On<MessageDTO>("ReceiveMessage", msg =>
            {
                OnMessageReceived?.Invoke(msg);
            });

            _connection.On<object>("UpdateConversationStatus", data =>
            {
                if (data is System.Text.Json.JsonElement json)
                {
                    var convId = json.GetProperty("conversationId").GetGuid();
                    var online = json.GetProperty("onlineCount").GetInt32();
                    var members = json.TryGetProperty("participantsCount", out var mc)
                        ? mc.GetInt32() : -1;
                    OnConversationStatusUpdated?.Invoke(convId, online, members);
                }
            });

            // Server notifies this user that someone started a chat with them
            _connection.On<Guid>("ConversationCreated", convId =>
            {
                OnConversationCreated?.Invoke(convId);
            });

            // On reconnect — reload conversations so nothing is stale
            _connection.Reconnected += _ =>
            {
                OnReconnected?.Invoke();
                return Task.CompletedTask;
            };

            await _connection.StartAsync();
        }

        // Call this after a new conversation is created so the hub re-joins all groups
        public async Task RestartAsync()
        {
            if (_connection != null)
            {
                await _connection.StopAsync();
                await _connection.DisposeAsync();
                _connection = null;
            }
            await StartAsync();
        }

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