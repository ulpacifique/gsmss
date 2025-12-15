using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityFinanceAPI.Models.Entities
{
    public class UserPermission : BaseEntity
    {
        [Key]
        public int UserPermissionId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int PermissionId { get; set; }

        public bool IsGranted { get; set; } = true; // Can be used to revoke permissions

        public DateTime? GrantedAt { get; set; }

        public int? GrantedBy { get; set; } // Admin who granted this permission

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("PermissionId")]
        public virtual Permission Permission { get; set; } = null!;

        [ForeignKey("GrantedBy")]
        public virtual User? GrantedByUser { get; set; }
    }
}


