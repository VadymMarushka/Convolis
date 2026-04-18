using Convolis.Api.Hubs;
using Convolis.Api.Services.Abstractions;
using Convolis.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace Convolis.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ConversationsController(
        IConversationService conversationService,
        IHubContext<ChatHub> hubContext) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<ConversationDTO>>> GetMyConversations()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();
            return Ok(await conversationService.GetUserConversationsAsync(userId.Value));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ConversationDetailsDTO>> GetConversation(Guid id)
        {
            var userId = GetUserId();
            var conversation = await conversationService.GetConversationByIdAsync(id, userId);
            if (conversation == null) return NotFound();
            return Ok(conversation);
        }

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

            // Notify the target user in real-time so the chat appears without reload
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