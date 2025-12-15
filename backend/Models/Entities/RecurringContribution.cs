using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityFinanceAPI.Models.Entities
{
    public class RecurringContribution : BaseEntity
    {
        [Key]
        public int RecurringContributionId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int GoalId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(50)]
        public string Frequency { get; set; } = "Monthly"; // Monthly, Weekly, BiWeekly

        public int DayOfMonth { get; set; } = 1; // For monthly: 1-31

        public DateTime? LastProcessedDate { get; set; }

        public DateTime? NextProcessDate { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("GoalId")]
        public virtual SavingsGoal Goal { get; set; } = null!;
    }
}


