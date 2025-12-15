using CommunityFinanceAPI.Models.DTOs;

namespace CommunityFinanceAPI.Services.Interfaces
{
    public interface INotificationService
    {
        Task<NotificationResponse> CreateNotificationAsync(CreateNotificationRequest request);
        Task<IEnumerable<NotificationResponse>> GetUserNotificationsAsync(int userId);
        Task<NotificationResponse> MarkAsReadAsync(int notificationId, int userId);
        Task MarkAllAsReadAsync(int userId);
        Task<int> GetUnreadCountAsync(int userId);
    }
}


