using CommunityFinanceAPI.Models.DTOs;

namespace CommunityFinanceAPI.Services.Interfaces
{
    public interface IContributionLimitService
    {
        Task<ContributionLimitResponse> CreateContributionLimitAsync(CreateContributionLimitRequest request);
        Task<ContributionLimitResponse> UpdateContributionLimitAsync(int limitId, UpdateContributionLimitRequest request);
        Task<ContributionLimitResponse?> GetContributionLimitByGoalIdAsync(int goalId);
        Task<IEnumerable<ContributionLimitResponse>> GetAllContributionLimitsAsync();
        Task<bool> DeleteContributionLimitAsync(int limitId);
    }
}


