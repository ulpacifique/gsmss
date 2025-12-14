

using CommunityFinanceAPI.Models.Entities;

namespace CommunityFinanceAPI.Services.Interfaces
{
    public interface IMemberService
    {
        Task<List<User>> GetMembersAsync();
        Task<User> GetMemberByIdAsync(int userId);
        Task<bool> UpdateMemberStatusAsync(int userId, bool isActive);
        Task<List<MemberGoal>> GetMemberGoalsAsync(int userId);
    }
}