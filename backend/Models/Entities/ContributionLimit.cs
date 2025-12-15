using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityFinanceAPI.Models.Entities
{
    public class ContributionLimit : BaseEntity
    {
        [Key]
        public int LimitId { get; set; }

        [Required]
        public int GoalId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? FixedAmount { get; set; } // If set, users must contribute exactly this amount

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MinimumAmount { get; set; } // Minimum contribution allowed

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MaximumAmount { get; set; } // Maximum contribution allowed per user

        [Column(TypeName = "decimal(18,2)")]
        public decimal? MaximumTotalPerUser { get; set; } // Maximum total contributions per user across all goals

        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey("GoalId")]
        public virtual SavingsGoal Goal { get; set; } = null!;
    }
}


