using CommunityFinanceAPI.Models.DTOs;

namespace CommunityFinanceAPI.Services.Interfaces
{
    public interface ILoanService
    {
        Task<LoanResponse> RequestLoanAsync(int userId, CreateLoanRequest request);
        Task<LoanResponse> ApproveLoanAsync(int loanId, int approvedBy, ApproveLoanRequest? request = null);
        Task<LoanResponse> RejectLoanAsync(int loanId, int rejectedBy, RejectLoanRequest request);
        Task<LoanResponse> PayLoanAsync(int loanId, int userId, PayLoanRequest request);
        Task<LoanResponse> GetLoanByIdAsync(int loanId);
        Task<IEnumerable<LoanResponse>> GetUserLoansAsync(int userId);
        Task<IEnumerable<LoanResponse>> GetPendingLoansAsync();
        Task<IEnumerable<LoanResponse>> GetActiveLoansAsync();
        Task<IEnumerable<LoanResponse>> GetAllLoansAsync();
        Task<MemberAccountResponse> GetMemberAccountAsync(int userId);
        Task<LoanStatsResponse> GetLoanStatsAsync();
        Task<decimal> GetTotalAccountBalanceAsync();
        Task<bool> HasOutstandingLoansAsync(int userId);
        Task<bool> HasTakenLoanThisYearAsync(int userId);
    }
}

