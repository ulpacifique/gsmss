namespace CommunityFinanceAPI.Models.DTOs
{
    public class LoanPaymentResponse
    {
        public int PaymentId { get; set; }
        public int LoanId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentReference { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}


