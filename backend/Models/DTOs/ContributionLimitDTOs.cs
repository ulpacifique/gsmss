using System.ComponentModel.DataAnnotations;

namespace CommunityFinanceAPI.Models.DTOs
{
    public class ContributionLimitResponse
    {
        public int LimitId { get; set; }
        public int GoalId { get; set; }
        public string GoalName { get; set; } = string.Empty;
        public decimal? FixedAmount { get; set; }
        public decimal? MinimumAmount { get; set; }
        public decimal? MaximumAmount { get; set; }
        public decimal? MaximumTotalPerUser { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateContributionLimitRequest
    {
        [Required]
        public int GoalId { get; set; }

        public decimal? FixedAmount { get; set; }
        public decimal? MinimumAmount { get; set; }
        public decimal? MaximumAmount { get; set; }
        public decimal? MaximumTotalPerUser { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateContributionLimitRequest
    {
        public decimal? FixedAmount { get; set; }
        public decimal? MinimumAmount { get; set; }
        public decimal? MaximumAmount { get; set; }
        public decimal? MaximumTotalPerUser { get; set; }
        public bool? IsActive { get; set; }
    }
}


