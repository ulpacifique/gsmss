using CommunityFinanceAPI.Models.DTOs;
using CommunityFinanceAPI.Models.Entities;

namespace CommunityFinanceAPI.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserResponse> GetUserByIdAsync(int userId);
        Task<UserResponse> GetUserByEmailAsync(string email);
        Task<IEnumerable<UserResponse>> GetAllUsersAsync();
        Task<IEnumerable<UserResponse>> GetUsersByRoleAsync(string role);
        Task<UserResponse> UpdateUserAsync(int userId, UpdateUserRequest request);
        Task<bool> DeactivateUserAsync(int userId);
        Task<bool> ActivateUserAsync(int userId);
        Task<UserStatsResponse> GetUserStatsAsync(int userId);
        Task<IEnumerable<MemberGoalResponse>> GetUserGoalsAsync(int userId);
    }
}