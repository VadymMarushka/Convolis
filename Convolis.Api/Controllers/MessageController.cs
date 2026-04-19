using Convolis.Api.Hubs;
using Convolis.Api.Services.Abstractions;
using Convolis.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Convolis.Api.Controllers
{
    /// <summary>
    /// Handles incoming chat messages, persisting them and broadcasting them in real-time via SignalR.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MessageController(
        IMessageService messageService,
        IHubContext<ChatHub> hubContext) : ControllerBase
    {
        /// <summary>
        /// Processes, saves, and broadcasts a new message to the specified conversation.
        /// </summary>
        /// <param name="request">The message payload containing the conversation ID and text content.</param>
        /// <returns>The fully processed message DTO, including AI sentiment analysis results.</returns>
        [HttpPost]
        public async Task<ActionResult<MessageDTO>> SendMessage([FromBody] SendMessageRequestDTO request)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            // Create the message. The service handles AI sentiment analysis internally.
            var savedMessage = await messageService.CreateMessageAsync(userId.Value, request.ConversationId, request.Text);

            if (savedMessage == null)
                return BadRequest("Failed to send message. Ensure you are a participant of this conversation.");

            // Broadcast the message (with its evaluated sentiment) to all connected clients in the conversation group
            var groupName = request.ConversationId.ToString();

            await hubContext.Clients.Group(groupName)
                .SendAsync("ReceiveMessage", savedMessage);

            return Ok(savedMessage);
        }

        private Guid? GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return Guid.TryParse(claim?.Value, out var id) ? id : null;
        }
    }
}