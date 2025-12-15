using System.ComponentModel.DataAnnotations;

namespace CommunityFinanceAPI.Models.DTOs
{
    public class ContributionResponse
    {
        public int ContributionId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int GoalId { get; set; }
        public string GoalName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentReference { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
        public int? ReviewedBy { get; set; }
        public string? ReviewedByName { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateContributionRequest
    {
        [Required]
        public int GoalId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [MaxLength(100)]
        public string? PaymentReference { get; set; } // Optional - will use member name if not provided
    }

    public class UpdateContributionStatusRequest
    {
        [Required]
        public string Status { get; set; } = string.Empty; // Approved, Rejected

        [MaxLength(500)]
        public string? RejectionReason { get; set; }
    }

    public class ContributionStatsResponse
    {
        public int TotalContributions { get; set; }
        public int PendingContributions { get; set; }
        public int ApprovedContributions { get; set; }
        public int RejectedContributions { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AverageContribution { get; set; }
        public decimal ThisMonthTotal { get; set; }
        public decimal LastMonthTotal { get; set; }
        public decimal PercentageChange { get; set; }
    }
}