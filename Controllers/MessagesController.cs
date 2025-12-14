using CommunityFinanceAPI.Models.DTOs;
using CommunityFinanceAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CommunityFinanceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageService _messageService;
        private readonly ILogger<MessagesController> _logger;

        public MessagesController(
            IMessageService messageService,
            ILogger<MessagesController> logger)
        {
            _messageService = messageService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] CreateMessageRequest request)
        {
            try
            {
                if (HttpContext.Items["AuthenticatedUser"] is not Models.Entities.User user)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var message = await _messageService.SendMessageAsync(user.UserId, request);
                return Ok(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message");
                return StatusCode(500, new { message = "An error occurred while sending message" });
            }
        }

        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversations()
        {
            try
            {
                if (HttpContext.Items["AuthenticatedUser"] is not Models.Entities.User user)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var conversations = await _messageService.GetConversationsAsync(user.UserId);
                return Ok(conversations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversations");
                return StatusCode(500, new { message = "An error occurred while retrieving conversations" });
            }
        }

        [HttpGet("conversation/{otherUserId}")]
        public async Task<IActionResult> GetConversation(int otherUserId)
        {
            try
            {
                if (HttpContext.Items["AuthenticatedUser"] is not Models.Entities.User user)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var messages = await _messageService.GetConversationAsync(user.UserId, otherUserId);
                
                // Mark conversation as read when viewing
                await _messageService.MarkConversationAsReadAsync(user.UserId, otherUserId);
                
                return Ok(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conversation");
                return StatusCode(500, new { message = "An error occurred while retrieving conversation" });
            }
        }

        [HttpPut("{messageId}/read")]
        public async Task<IActionResult> MarkAsRead(int messageId)
        {
            try
            {
                if (HttpContext.Items["AuthenticatedUser"] is not Models.Entities.User user)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var message = await _messageService.MarkAsReadAsync(messageId, user.UserId);
                return Ok(message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking message as read");
                return StatusCode(500, new { message = "An error occurred while updating message" });
            }
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                if (HttpContext.Items["AuthenticatedUser"] is not Models.Entities.User user)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var count = await _messageService.GetUnreadCountAsync(user.UserId);
                return Ok(new { count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread count");
                return StatusCode(500, new { message = "An error occurred while retrieving unread count" });
            }
        }
    }
}

