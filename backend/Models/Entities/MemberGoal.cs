using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityFinanceAPI.Models.Entities
{
    public class MemberGoal
    {
        [Key]
        public int MemberGoalId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int GoalId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PersonalTarget { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentAmount { get; set; }

        public DateTime JoinedAt { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("GoalId")]
        public virtual SavingsGoal Goal { get; set; } = null!;

        [NotMapped]
        public decimal PersonalProgressPercentage => PersonalTarget > 0 ? (CurrentAmount / PersonalTarget) * 100 : 0;
    }
}