using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityFinanceAPI.Models.Entities
{
    public class ContributionReward : BaseEntity
    {
        [Key]
        public int RewardId { get; set; }

        [Required]
        [MaxLength(100)]
        public string RewardName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ContributionThreshold { get; set; } // Minimum contribution amount to earn this reward

        [Column(TypeName = "decimal(18,2)")]
        public decimal? RewardAmount { get; set; } // Cash reward amount (if applicable)

        [MaxLength(200)]
        public string? RewardType { get; set; } // "Cash", "Discount", "Bonus", "Recognition", etc.

        public bool IsActive { get; set; } = true;

        public DateTime? ValidFrom { get; set; }

        public DateTime? ValidUntil { get; set; }
    }
}


