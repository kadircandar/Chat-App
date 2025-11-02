using ChatApp.Api.DTOs.Messages;
using ChatApp.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChatApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MessagesController : ControllerBase
    {

        private readonly IMessageService _messageService;

        public MessagesController(IMessageService messageService)
        {
            _messageService = messageService;
        }

        // POST: api/messages
        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageDto dto)
        {
            var result = await _messageService.SendMessage(dto);

            if (result.Success) { return Ok(); }

            return BadRequest(result.Message);
        }

        // GET: api/messages/history/{friendUserId}
        [HttpGet("history/{friendUserId}")]
        public async Task<ActionResult<List<GetMessageHistoryDto>>> GetMessageHistory(string friendUserId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var result = await _messageService.GetMessageHistory(userId, friendUserId);

            if (result.Success) { return Ok(result.Data); }

            return BadRequest(result.Message);

        }

        // DELETE: api/messages/{messageId}
        [HttpDelete("{messageId}")]
        public async Task<IActionResult> DeleteMessage(string messageId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var result = await _messageService.DeleteMessage(userId, messageId);

            if (result.Success) { return Ok(); }

            return BadRequest(result.Message);
        }

        // POST: api/messages/markasread
        [HttpPost("markasread")]
        public async Task<IActionResult> MarkAsRead([FromBody] MarkAsReadDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var result = await _messageService.MarkAsRead(userId, dto);

            if (result.Success) { return Ok(); }

            return BadRequest(result.Message);
        }
    }
}
