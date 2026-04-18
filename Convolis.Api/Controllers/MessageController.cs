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
    public class MessageController(
        IMessageService messageService,
        IHubContext<ChatHub> hubContext) : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult<MessageDTO>> SendMessage([FromBody] SendMessageRequestDTO request)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var savedMessage = await messageService.CreateMessageAsync(userId.Value, request.ConversationId, request.Text);

            if (savedMessage == null) return BadRequest("Не вдалося зберегти повідомлення");
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