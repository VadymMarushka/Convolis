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
    /// Manages chat conversations, including creation, retrieval, and real-time notifications via SignalR.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ConversationsController(
        IConversationService conversationService,
        IHubContext<ChatHub> hubContext) : ControllerBase
    {
        /// <summary>
        /// Retrieves all conversations for the currently authenticated user.
        /// </summary>
        /// <returns>A list of conversation summaries including online counts.</returns>
        [HttpGet]
        public async Task<ActionResult<List<ConversationDTO>>> GetMyConversations()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();
            return Ok(await conversationService.GetUserConversationsAsync(userId.Value));
        }

        /// <summary>
        /// Retrieves the full details and message history of a specific conversation.
        /// </summary>
        /// <param name="id">The unique identifier of the conversation.</param>
        /// <returns>The conversation details, or a 404 Not Found if it doesn't exist.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ConversationDetailsDTO>> GetConversation(Guid id)
        {
            var userId = GetUserId();
            var conversation = await conversationService.GetConversationByIdAsync(id, userId);
            if (conversation == null) return NotFound();
            return Ok(conversation);
        }

        /// <summary>
        /// Creates a new 1-on-1 chat with another user by their username.
        /// </summary>
        /// <param name="targetUsername">The username of the target user.</param>
        /// <returns>The newly created conversation data, or a 400 Bad Request if validation fails.</returns>
        [HttpPost]
        public async Task<ActionResult<ConversationDTO>> CreateChat([FromQuery] string targetUsername)
        {
            if (string.IsNullOrWhiteSpace(targetUsername))
                return BadRequest("Username cannot be empty.");

            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var (newConversation, targetUserId) =
                await conversationService.CreateChatByUsernameAsync(userId.Value, targetUsername);

            if (newConversation == null)
                return BadRequest("User not found or you cannot chat with yourself.");

            // Trigger a real-time SignalR event to the target user's active connections 
            // so the new chat appears in their UI instantly without requiring a page reload.
            if (targetUserId.HasValue)
            {
                await hubContext.Clients
                    .User(targetUserId.Value.ToString())
                    .SendAsync("ConversationCreated", newConversation.Id);
            }

            return Ok(newConversation);
        }

        private Guid? GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return Guid.TryParse(claim?.Value, out var id) ? id : null;
        }
    }
}