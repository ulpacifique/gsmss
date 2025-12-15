using System.ComponentModel.DataAnnotations;

namespace CommunityFinanceAPI.Models.DTOs
{
    public class LoanResponse
    {
        public int LoanId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public decimal PrincipalAmount { get; set; }
        public decimal InterestRate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public DateTime RequestedDate { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int? ApprovedBy { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? RejectionReason { get; set; }
        public bool IsOverdue { get; set; }
        public int DaysUntilDue { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateLoanRequest
    {
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [MaxLength(500)]
        public string? Purpose { get; set; }
    }

    public class ApproveLoanRequest
    {
        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class RejectLoanRequest
    {
        [Required]
        [MaxLength(500)]
        public string RejectionReason { get; set; } = string.Empty;
    }

    public class PayLoanRequest
    {
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(100)]
        public string PaymentReference { get; set; } = string.Empty;
    }

    public class MemberAccountResponse
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public decimal AccountBalance { get; set; } // Total contributions
        public decimal TotalLoansTaken { get; set; }
        public decimal TotalLoansPaid { get; set; }
        public decimal OutstandingLoanAmount { get; set; }
        public int ActiveLoansCount { get; set; }
        public int OverdueLoansCount { get; set; }
        public List<LoanResponse> ActiveLoans { get; set; } = new();
        public string ContributionTier { get; set; } = "Bronze"; // Bronze, Silver, Gold, Platinum
        public decimal MaxLoanAmount { get; set; } // Maximum loan amount (minimum of: contributions×multiplier OR percentage of total balance, capped by available funds)
        public decimal MaxLoanPercentage { get; set; } // Max loan percentage of total balance for this tier
    }

    public class LoanStatsResponse
    {
        public decimal TotalAccountBalance { get; set; }
        public decimal TotalLoansOutstanding { get; set; }
        public decimal TotalLoansPaid { get; set; }
        public decimal TotalInterestEarned { get; set; }
        public int PendingLoanRequests { get; set; }
        public int ActiveLoans { get; set; }
        public int OverdueLoans { get; set; }
        public decimal MaxIndividualLoanAmount { get; set; } // 12.5% of balance
    }
}

