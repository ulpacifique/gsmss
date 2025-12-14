using CommunityFinanceAPI.Data;
using CommunityFinanceAPI.Models.DTOs;
using CommunityFinanceAPI.Models.Entities;
using CommunityFinanceAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CommunityFinanceAPI.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserResponse> GetUserByIdAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.MemberGoals)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                throw new KeyNotFoundException("User not found");

            return MapToUserResponse(user);
        }


        public async Task<UserResponse> GetUserByEmailAsync(string email)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                throw new KeyNotFoundException("User not found");

            return MapToUserResponse(user);
        }

        public async Task<IEnumerable<UserResponse>> GetAllUsersAsync()
        {
            var users = await _context.Users
                .Where(u => u.IsActive)
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToListAsync();

            return users.Select(MapToUserResponse);
        }

        public async Task<IEnumerable<UserResponse>> GetUsersByRoleAsync(string role)
        {
            var users = await _context.Users
                .Where(u => u.Role == role && u.IsActive)
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToListAsync();

            return users.Select(MapToUserResponse);
        }

        public async Task<UserResponse> UpdateUserAsync(int userId, UpdateUserRequest request)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            if (!string.IsNullOrWhiteSpace(request.FirstName))
                user.FirstName = request.FirstName;

            if (!string.IsNullOrWhiteSpace(request.LastName))
                user.LastName = request.LastName;

            if (request.PhoneNumber != null)
                user.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber;

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                // Check if email is already taken by another user
                var emailExists = await _context.Users
                    .AnyAsync(u => u.Email == request.Email && u.UserId != userId);
                if (emailExists)
                    throw new InvalidOperationException("Email is already taken by another user");
                user.Email = request.Email;
            }

            if (request.ProfilePictureUrl != null)
                user.ProfilePictureUrl = string.IsNullOrWhiteSpace(request.ProfilePictureUrl) ? null : request.ProfilePictureUrl;

            user.UpdatedAt = DateTime.UtcNow;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return MapToUserResponse(user);
        }

        public async Task<bool> DeactivateUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            user.IsActive = false;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ActivateUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            user.IsActive = true;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<UserStatsResponse> GetUserStatsAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.MemberGoals)
                .Include(u => u.Contributions)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                throw new KeyNotFoundException("User not found");

            var totalGoals = user.MemberGoals.Count;
            var activeGoals = user.MemberGoals.Count(mg => mg.Goal.Status == "Active");
            var totalContributed = user.Contributions
                .Where(c => c.Status == "Approved")
                .Sum(c => c.Amount);

            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;
            var currentMonthContribution = user.Contributions
                .Where(c => c.Status == "Approved" &&
                           c.CreatedAt.Month == currentMonth &&
                           c.CreatedAt.Year == currentYear)
                .Sum(c => c.Amount);

            return new UserStatsResponse
            {
                TotalGoals = totalGoals,
                ActiveGoals = activeGoals,
                TotalContributed = totalContributed,
                CurrentMonthContribution = currentMonthContribution
            };
        }

        public async Task<IEnumerable<MemberGoalResponse>> GetUserGoalsAsync(int userId)
        {
            // Use projection to avoid Include() which might hit invalid shadow properties
            var memberGoals = await _context.MemberGoals
                .AsNoTracking()
                .Where(mg => mg.UserId == userId)
                .OrderByDescending(mg => mg.JoinedAt)
                .Select(mg => new MemberGoalResponse
                {
                    MemberGoalId = mg.MemberGoalId,
                    UserId = mg.UserId,
                    UserName = mg.User != null ? $"{mg.User.FirstName} {mg.User.LastName}" : "Unknown",
                    GoalId = mg.GoalId,
                    GoalName = mg.Goal != null ? mg.Goal.GoalName : "Unknown",
                    PersonalTarget = mg.PersonalTarget,
                    CurrentAmount = mg.CurrentAmount,
                    PersonalProgressPercentage = mg.PersonalTarget > 0 ? (mg.CurrentAmount / mg.PersonalTarget) * 100 : 0,
                    JoinedAt = mg.JoinedAt
                })
                .ToListAsync();

            return memberGoals;
        }

        private UserResponse MapToUserResponse(User user)
        {
            return new UserResponse
            {
                UserId = user.UserId,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                ProfilePictureUrl = user.ProfilePictureUrl,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }
    }
}