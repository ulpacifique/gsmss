using CommunityFinanceAPI.Models.DTOs;

namespace CommunityFinanceAPI.Services.Interfaces
{
    public interface ILoanRiskAssessmentService
    {
        Task<LoanRiskAssessmentResponse> AssessLoanRiskAsync(int userId, decimal requestedAmount, string loanPurpose);
        Task<LoanRiskScoreResponse> GetUserRiskScoreAsync(int userId);
    }
}


