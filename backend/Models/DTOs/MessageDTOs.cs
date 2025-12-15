namespace CommunityFinanceAPI.Models.DTOs
{
    public class MessageResponse
    {
        public int MessageId { get; set; }
        public int SenderId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string? SenderEmail { get; set; }
        public int ReceiverId { get; set; }
        public string ReceiverName { get; set; } = string.Empty;
        public string? ReceiverEmail { get; set; }
        public string Content { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public string? MessageType { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateMessageRequest
    {
        public int ReceiverId { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? MessageType { get; set; } = "chat";
    }

    public class ConversationResponse
    {
        public int OtherUserId { get; set; }
        public string OtherUserName { get; set; } = string.Empty;
        public string? OtherUserEmail { get; set; }
        public string? OtherUserProfilePicture { get; set; }
        public MessageResponse? LastMessage { get; set; }
        public int UnreadCount { get; set; }
    }
}


