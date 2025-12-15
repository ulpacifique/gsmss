using System.ComponentModel.DataAnnotations;

namespace CommunityFinanceAPI.Models.DTOs
{
    public class GoalResponse
    {
        public int GoalId { get; set; }
        public string GoalName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal TargetAmount { get; set; }
        public decimal CurrentAmount { get; set; }
        public decimal ProgressPercentage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int MemberCount { get; set; }
        public bool IsOverdue { get; set; }
        public int DaysRemaining { get; set; }
    }

    public class CreateGoalRequest
    {
        [Required]
        [MaxLength(100)]
        public string GoalName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal TargetAmount { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }
    }

    public class UpdateGoalRequest
    {
        [MaxLength(100)]
        public string? GoalName { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal? TargetAmount { get; set; }

        public DateTime? EndDate { get; set; }

        public string? Status { get; set; }
    }

    public class GoalStatsResponse
    {
        public int TotalGoals { get; set; }
        public int ActiveGoals { get; set; }
        public int CompletedGoals { get; set; }
        public int OverdueGoals { get; set; }
        public decimal TotalTargetAmount { get; set; }
        public decimal TotalCurrentAmount { get; set; }
        public decimal OverallProgressPercentage { get; set; }
    }
}