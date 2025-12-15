using CommunityFinanceAPI.Models.DTOs;

namespace CommunityFinanceAPI.Services.Interfaces
{
    public interface IContributionService
    {
        Task<ContributionResponse> CreateContributionAsync(CreateContributionRequest request, int userId);
        Task<ContributionResponse> GetContributionByIdAsync(int contributionId);
        Task<IEnumerable<ContributionResponse>> GetAllContributionsAsync();
        Task<IEnumerable<ContributionResponse>> GetContributionsByUserAsync(int userId);
        Task<IEnumerable<ContributionResponse>> GetContributionsByGoalAsync(int goalId);
        Task<IEnumerable<ContributionResponse>> GetContributionsByStatusAsync(string status);
        Task<IEnumerable<ContributionResponse>> GetPendingContributionsAsync();
        Task<ContributionResponse> UpdateContributionStatusAsync(int contributionId, UpdateContributionStatusRequest request, int reviewedBy);
        Task<bool> DeleteContributionAsync(int contributionId);
        Task<ContributionStatsResponse> GetContributionStatsAsync();
        Task<ContributionStatsResponse> GetUserContributionStatsAsync(int userId);
    }
}