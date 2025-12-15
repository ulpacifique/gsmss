namespace CommunityFinanceAPI.Models.DTOs
{
    public class UserResponse
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public string? PhoneNumber { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UpdateUserRequest
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? ProfilePictureUrl { get; set; }
    }

    public class UserStatsResponse
    {
        public int TotalGoals { get; set; }
        public int ActiveGoals { get; set; }
        public decimal TotalContributed { get; set; }
        public decimal CurrentMonthContribution { get; set; }
    }
}