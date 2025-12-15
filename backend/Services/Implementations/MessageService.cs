using CommunityFinanceAPI.Data;
using CommunityFinanceAPI.Models.DTOs;
using CommunityFinanceAPI.Models.Entities;
using CommunityFinanceAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CommunityFinanceAPI.Services.Implementations
{
    public class MessageService : IMessageService
    {
        private readonly ApplicationDbContext _context;

        public MessageService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<MessageResponse> SendMessageAsync(int senderId, CreateMessageRequest request)
        {
            var message = new Message
            {
                SenderId = senderId,
                ReceiverId = request.ReceiverId,
                Content = request.Content,
                MessageType = request.MessageType ?? "chat",
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return await GetMessageByIdAsync(message.MessageId);
        }

        public async Task<IEnumerable<MessageResponse>> GetConversationAsync(int userId1, int userId2)
        {
            return await _context.Messages
                .AsNoTracking()
                .Where(m => (m.SenderId == userId1 && m.ReceiverId == userId2) ||
                           (m.SenderId == userId2 && m.ReceiverId == userId1))
                .OrderBy(m => m.CreatedAt)
                .Select(m => new MessageResponse
                {
                    MessageId = m.MessageId,
                    SenderId = m.SenderId,
                    SenderName = m.Sender != null ? $"{m.Sender.FirstName} {m.Sender.LastName}" : "Unknown",
                    SenderEmail = m.Sender != null ? m.Sender.Email : null,
                    ReceiverId = m.ReceiverId,
                    ReceiverName = m.Receiver != null ? $"{m.Receiver.FirstName} {m.Receiver.LastName}" : "Unknown",
                    ReceiverEmail = m.Receiver != null ? m.Receiver.Email : null,
                    Content = m.Content,
                    IsRead = m.IsRead,
                    ReadAt = m.ReadAt,
                    MessageType = m.MessageType,
                    CreatedAt = m.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<ConversationResponse>> GetConversationsAsync(int userId)
        {
            // Get all unique conversations for this user with last message details
            var conversations = await _context.Messages
                .AsNoTracking()
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
                .Select(g => new
                {
                    OtherUserId = g.Key,
                    LastMessageId = g.OrderByDescending(m => m.CreatedAt).Select(m => m.MessageId).FirstOrDefault(),
                    LastMessageContent = g.OrderByDescending(m => m.CreatedAt).Select(m => m.Content).FirstOrDefault(),
                    LastMessageCreatedAt = g.OrderByDescending(m => m.CreatedAt).Select(m => m.CreatedAt).FirstOrDefault(),
                    LastMessageSenderId = g.OrderByDescending(m => m.CreatedAt).Select(m => m.SenderId).FirstOrDefault(),
                    LastMessageReceiverId = g.OrderByDescending(m => m.CreatedAt).Select(m => m.ReceiverId).FirstOrDefault(),
                    LastMessageIsRead = g.OrderByDescending(m => m.CreatedAt).Select(m => m.IsRead).FirstOrDefault(),
                    LastMessageType = g.OrderByDescending(m => m.CreatedAt).Select(m => m.MessageType).FirstOrDefault()
                })
                .ToListAsync();

            var result = new List<ConversationResponse>();

            foreach (var conv in conversations)
            {
                var otherUser = await _context.Users
                    .AsNoTracking()
                    .Where(u => u.UserId == conv.OtherUserId)
                    .Select(u => new
                    {
                        u.UserId,
                        u.FirstName,
                        u.LastName,
                        u.Email,
                        u.ProfilePictureUrl
                    })
                    .FirstOrDefaultAsync();

                if (otherUser == null) continue;

                var unreadCount = await _context.Messages
                    .CountAsync(m => m.SenderId == conv.OtherUserId && 
                                    m.ReceiverId == userId && 
                                    !m.IsRead);

                // Get sender and receiver names for last message
                var lastMessageSender = await _context.Users
                    .AsNoTracking()
                    .Where(u => u.UserId == conv.LastMessageSenderId)
                    .Select(u => $"{u.FirstName} {u.LastName}")
                    .FirstOrDefaultAsync() ?? "Unknown";

                var lastMessageReceiver = await _context.Users
                    .AsNoTracking()
                    .Where(u => u.UserId == conv.LastMessageReceiverId)
                    .Select(u => $"{u.FirstName} {u.LastName}")
                    .FirstOrDefaultAsync() ?? "Unknown";

                result.Add(new ConversationResponse
                {
                    OtherUserId = otherUser.UserId,
                    OtherUserName = $"{otherUser.FirstName} {otherUser.LastName}",
                    OtherUserEmail = otherUser.Email,
                    OtherUserProfilePicture = otherUser.ProfilePictureUrl,
                    LastMessage = conv.LastMessageId > 0 ? new MessageResponse
                    {
                        MessageId = conv.LastMessageId,
                        SenderId = conv.LastMessageSenderId,
                        SenderName = lastMessageSender,
                        ReceiverId = conv.LastMessageReceiverId,
                        ReceiverName = lastMessageReceiver,
                        Content = conv.LastMessageContent ?? "",
                        IsRead = conv.LastMessageIsRead,
                        MessageType = conv.LastMessageType,
                        CreatedAt = conv.LastMessageCreatedAt
                    } : null,
                    UnreadCount = unreadCount
                });
            }

            return result.OrderByDescending(c => c.LastMessage?.CreatedAt ?? DateTime.MinValue);
        }

        public async Task<MessageResponse> MarkAsReadAsync(int messageId, int userId)
        {
            var message = await _context.Messages
                .FirstOrDefaultAsync(m => m.MessageId == messageId && m.ReceiverId == userId);

            if (message == null)
                throw new KeyNotFoundException("Message not found");

            message.IsRead = true;
            message.ReadAt = DateTime.UtcNow;
            message.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return await GetMessageByIdAsync(messageId);
        }

        public async Task MarkConversationAsReadAsync(int userId, int otherUserId)
        {
            var messages = await _context.Messages
                .Where(m => m.SenderId == otherUserId && m.ReceiverId == userId && !m.IsRead)
                .ToListAsync();

            foreach (var message in messages)
            {
                message.IsRead = true;
                message.ReadAt = DateTime.UtcNow;
                message.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _context.Messages
                .CountAsync(m => m.ReceiverId == userId && !m.IsRead);
        }

        private async Task<MessageResponse> GetMessageByIdAsync(int messageId)
        {
            return await _context.Messages
                .AsNoTracking()
                .Where(m => m.MessageId == messageId)
                .Select(m => new MessageResponse
                {
                    MessageId = m.MessageId,
                    SenderId = m.SenderId,
                    SenderName = m.Sender != null ? $"{m.Sender.FirstName} {m.Sender.LastName}" : "Unknown",
                    SenderEmail = m.Sender != null ? m.Sender.Email : null,
                    ReceiverId = m.ReceiverId,
                    ReceiverName = m.Receiver != null ? $"{m.Receiver.FirstName} {m.Receiver.LastName}" : "Unknown",
                    ReceiverEmail = m.Receiver != null ? m.Receiver.Email : null,
                    Content = m.Content,
                    IsRead = m.IsRead,
                    ReadAt = m.ReadAt,
                    MessageType = m.MessageType,
                    CreatedAt = m.CreatedAt
                })
                .FirstOrDefaultAsync() ?? throw new KeyNotFoundException("Message not found");
        }
    }
}

