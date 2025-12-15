using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityFinanceAPI.Models.Entities
{
    public class SavingsGoal : BaseEntity
    {
        [Key]
        public int GoalId { get; set; }

        [Required]
        [MaxLength(100)]
        public string GoalName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TargetAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentAmount { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public string Status { get; set; } = "Active"; // Active, Completed, Cancelled

        [Required]
        public int CreatedBy { get; set; }

        // Navigation properties
        [ForeignKey("CreatedBy")]
        public virtual User CreatedByUser { get; set; } = null!;
        public virtual ICollection<Contribution> Contributions { get; set; } = new List<Contribution>();
        public virtual ICollection<MemberGoal> MemberGoals { get; set; } = new List<MemberGoal>();

        [NotMapped]
        public decimal ProgressPercentage => TargetAmount > 0 ? (CurrentAmount / TargetAmount) * 100 : 0;

        [NotMapped]
        public bool IsOverdue => Status == "Active" && DateTime.Now > EndDate;
    }
}