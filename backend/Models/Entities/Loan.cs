using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityFinanceAPI.Models.Entities
{
    public class Loan : BaseEntity
    {
        [Key]
        public int LoanId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrincipalAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal InterestRate { get; set; } = 5.0m; // 5% default

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; } // Principal + Interest

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal RemainingAmount { get; set; }

        [Required]
        public DateTime RequestedDate { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PaidAmount { get; set; } = 0;

        [Required]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, Paid, Overdue

        public int? ApprovedBy { get; set; } // Secretary/Admin who approved

        public DateTime? ApprovedAt { get; set; }

        public string? RejectionReason { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("ApprovedBy")]
        public virtual User? ApprovedByUser { get; set; }

        // Navigation property for loan payments
        public virtual ICollection<LoanPayment> LoanPayments { get; set; } = new List<LoanPayment>();

        [NotMapped]
        public bool IsOverdue => Status == "Approved" && DateTime.Now > DueDate && RemainingAmount > 0;

        [NotMapped]
        public int DaysUntilDue => Status == "Approved" ? (int)(DueDate - DateTime.Now).TotalDays : 0;
    }
}

