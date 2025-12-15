using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityFinanceAPI.Models.Entities
{
    public class Group : BaseEntity
    {
        [Key]
        public int GroupId { get; set; }

        [Required]
        [MaxLength(100)]
        public string GroupName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public int CreatedBy { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey("CreatedBy")]
        public virtual User Creator { get; set; } = null!;

        public virtual ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();
    }

    public class GroupMember : BaseEntity
    {
        [Key]
        public int GroupMemberId { get; set; }

        [Required]
        public int GroupId { get; set; }

        [Required]
        public int UserId { get; set; }

        public DateTime JoinedAt { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MonthlyContributionAmount { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey("GroupId")]
        public virtual Group Group { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}


