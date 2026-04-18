using Convolis.Api.Services.Abstractions;
using Convolis.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Convolis.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ConversationsController(IConversationService conversationService) : ControllerBase
    {
        // Отримати всі чати користувача
        [HttpGet]
        public async Task<ActionResult<List<ConversationDTO>>> GetMyConversations()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var conversations = await conversationService.GetUserConversationsAsync(userId.Value);
            return Ok(conversations);
        }

        // Отримати конкретний чат з повідомленнями
        [HttpGet("{id}")]
        public async Task<ActionResult<ConversationDetailsDTO>> GetConversation(Guid id)
        {
            var conversation = await conversationService.GetConversationByIdAsync(id);
            if (conversation == null) return NotFound();

            return Ok(conversation);
        }

        // Створити новий чат за нікнеймом
        [HttpPost]
        public async Task<ActionResult<ConversationDTO>> CreateChat([FromQuery] string targetUsername)
        {
            if (string.IsNullOrWhiteSpace(targetUsername))
                return BadRequest("Username cannot be empty.");

            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var newConversation = await conversationService.CreateChatByUsernameAsync(userId.Value, targetUsername);

            if (newConversation == null)
            {
                return BadRequest("User not found or you cannot chat with yourself.");
            }

            return Ok(newConversation);
        }

        // Хелпер для витягування ID з JWT токена
        private Guid? GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return Guid.TryParse(claim?.Value, out var id) ? id : null;
        }
    }
}