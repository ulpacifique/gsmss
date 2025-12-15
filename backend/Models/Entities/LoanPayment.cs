using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CommunityFinanceAPI.Models.Entities
{
    public class LoanPayment : BaseEntity
    {
        [Key]
        public int PaymentId { get; set; }

        [Required]
        public int LoanId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(100)]
        public string PaymentReference { get; set; } = string.Empty;

        [Required]
        public DateTime PaymentDate { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        // Navigation properties
        [ForeignKey("LoanId")]
        public virtual Loan Loan { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}


