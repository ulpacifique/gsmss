using CommunityFinanceAPI.Models.DTOs;
using CommunityFinanceAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CommunityFinanceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(
            INotificationService notificationService,
            ILogger<NotificationsController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyNotifications()
        {
            try
            {
                if (HttpContext.Items["AuthenticatedUser"] is not Models.Entities.User user)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var notifications = await _notificationService.GetUserNotificationsAsync(user.UserId);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications");
                return StatusCode(500, new { message = "An error occurred while retrieving notifications" });
            }
        }

        [HttpPut("{notificationId}/read")]
        public async Task<IActionResult> MarkAsRead(int notificationId)
        {
            try
            {
                if (HttpContext.Items["AuthenticatedUser"] is not Models.Entities.User user)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var notification = await _notificationService.MarkAsReadAsync(notificationId, user.UserId);
                return Ok(notification);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read");
                return StatusCode(500, new { message = "An error occurred while updating notification" });
            }
        }

        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                if (HttpContext.Items["AuthenticatedUser"] is not Models.Entities.User user)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                await _notificationService.MarkAllAsReadAsync(user.UserId);
                return Ok(new { message = "All notifications marked as read" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read");
                return StatusCode(500, new { message = "An error occurred while updating notifications" });
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

                var count = await _notificationService.GetUnreadCountAsync(user.UserId);
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

