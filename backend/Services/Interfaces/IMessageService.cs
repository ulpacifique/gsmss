using CommunityFinanceAPI.Models.DTOs;

namespace CommunityFinanceAPI.Services.Interfaces
{
    public interface IMessageService
    {
        Task<MessageResponse> SendMessageAsync(int senderId, CreateMessageRequest request);
        Task<IEnumerable<MessageResponse>> GetConversationAsync(int userId1, int userId2);
        Task<IEnumerable<ConversationResponse>> GetConversationsAsync(int userId);
        Task<MessageResponse> MarkAsReadAsync(int messageId, int userId);
        Task MarkConversationAsReadAsync(int userId, int otherUserId);
        Task<int> GetUnreadCountAsync(int userId);
    }
}


