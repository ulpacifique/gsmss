using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityFinanceAPI.Models.Entities
{
    public class Contribution : BaseEntity
    {
        [Key]
        public int ContributionId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int GoalId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(100)]
        public string PaymentReference { get; set; } = string.Empty;

        [Required]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

        public DateTime SubmittedAt { get; set; }

        public int? ReviewedBy { get; set; }

        public DateTime? ReviewedAt { get; set; }

        [MaxLength(500)]
        public string? RejectionReason { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("GoalId")]
        public virtual SavingsGoal Goal { get; set; } = null!;

        [ForeignKey("ReviewedBy")]
        public virtual User? ReviewedByUser { get; set; }
    }
}