using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityFinanceAPI.Models.Entities
{
    [Table("Users")] // ← ADD THIS LINE HERE
    public class User : BaseEntity
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        public string? ProfilePictureUrl { get; set; }

        [Required]
        public string Role { get; set; } = "Member"; // Member, Admin

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<SavingsGoal> CreatedGoals { get; set; } = new List<SavingsGoal>();
        public virtual ICollection<Contribution> Contributions { get; set; } = new List<Contribution>();
        public virtual ICollection<MemberGoal> MemberGoals { get; set; } = new List<MemberGoal>();
        public virtual ICollection<Loan> Loans { get; set; } = new List<Loan>();
        public virtual ICollection<Loan> ApprovedLoans { get; set; } = new List<Loan>();

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";
    }
}