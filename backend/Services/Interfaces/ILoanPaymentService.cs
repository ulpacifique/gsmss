using CommunityFinanceAPI.Models.DTOs;

namespace CommunityFinanceAPI.Services.Interfaces
{
    public interface ILoanPaymentService
    {
        Task<IEnumerable<LoanPaymentResponse>> GetLoanPaymentsAsync(int loanId);
        Task<IEnumerable<LoanPaymentResponse>> GetUserLoanPaymentsAsync(int userId);
        Task<LoanPaymentResponse> GetLoanPaymentByIdAsync(int paymentId);
    }
}


