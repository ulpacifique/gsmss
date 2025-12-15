namespace CommunityFinanceAPI.Models.DTOs
{
    public class MemberGoalResponse
    {
        public int MemberGoalId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int GoalId { get; set; }
        public string GoalName { get; set; } = string.Empty;
        public decimal PersonalTarget { get; set; }
        public decimal CurrentAmount { get; set; }
        public decimal PersonalProgressPercentage { get; set; }
        public DateTime JoinedAt { get; set; }
    }

    public class JoinGoalRequest
    {
        public decimal? PersonalTarget { get; set; }
    }

    public class UpdateMemberGoalRequest
    {
        public decimal? PersonalTarget { get; set; }
    }
}