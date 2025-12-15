using CommunityFinanceAPI.Models.DTOs;

namespace CommunityFinanceAPI.Services.Interfaces
{
    public interface IGoalService
    {
        Task<GoalResponse> CreateGoalAsync(CreateGoalRequest request, int createdBy);
        Task<GoalResponse> GetGoalByIdAsync(int goalId);
        Task<IEnumerable<GoalResponse>> GetAllGoalsAsync();
        Task<IEnumerable<GoalResponse>> GetGoalsByStatusAsync(string status);
        Task<IEnumerable<GoalResponse>> GetActiveGoalsAsync();
        Task<GoalResponse> UpdateGoalAsync(int goalId, UpdateGoalRequest request);
        Task<bool> DeleteGoalAsync(int goalId);
        Task<GoalStatsResponse> GetGoalStatsAsync();
        Task<MemberGoalResponse> JoinGoalAsync(int goalId, int userId, JoinGoalRequest request);
        Task<bool> LeaveGoalAsync(int goalId, int userId);
        Task<IEnumerable<MemberGoalResponse>> GetGoalMembersAsync(int goalId);
        Task<GoalResponse> UpdateGoalProgressAsync(int goalId);
    }
}