using System.ComponentModel.DataAnnotations;

namespace CommunityFinanceAPI.Models.Entities
{
    public class Permission : BaseEntity
    {
        [Key]
        public int PermissionId { get; set; }

        [Required]
        [MaxLength(100)]
        public string PermissionName { get; set; } = string.Empty; // e.g., "ManageUsers", "ApproveLoans", "ViewReports"

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        [MaxLength(50)]
        public string Category { get; set; } = string.Empty; // e.g., "UserManagement", "LoanManagement", "Reports"

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
    }
}


