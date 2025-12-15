using CommunityFinanceAPI.Data;
using CommunityFinanceAPI.Models.DTOs;
using CommunityFinanceAPI.Models.Entities;
using CommunityFinanceAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CommunityFinanceAPI.Services.Implementations
{
    public class GoalService : IGoalService
    {
        private readonly ApplicationDbContext _context;

        public GoalService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<GoalResponse> CreateGoalAsync(CreateGoalRequest request, int createdBy)
        {
            if (request.EndDate <= request.StartDate)
                throw new ArgumentException("End date must be after start date");

            var goal = new SavingsGoal
            {
                GoalName = request.GoalName,
                Description = request.Description,
                TargetAmount = request.TargetAmount,
                CurrentAmount = 0,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Status = "Active",
                CreatedBy = createdBy
            };

            _context.SavingsGoals.Add(goal);
            await _context.SaveChangesAsync();

            return await GetGoalByIdAsync(goal.GoalId);
        }

        public async Task<GoalResponse> GetGoalByIdAsync(int goalId)
        {
            // Use projection to avoid loading ProfilePictureUrl column that may not exist
            var goalData = await _context.SavingsGoals
                .AsNoTracking()
                .Where(g => g.GoalId == goalId)
                .Select(g => new
                {
                    g.GoalId,
                    g.GoalName,
                    g.Description,
                    g.TargetAmount,
                    g.CurrentAmount,
                    g.StartDate,
                    g.EndDate,
                    g.Status,
                    g.CreatedBy,
                    g.CreatedAt,
                    g.UpdatedAt,
                    CreatedByUser = g.CreatedByUser != null ? new
                    {
                        g.CreatedByUser.UserId,
                        g.CreatedByUser.FirstName,
                        g.CreatedByUser.LastName,
                        g.CreatedByUser.Email
                    } : null,
                    MemberGoalsCount = g.MemberGoals.Count,
                    MemberGoals = g.MemberGoals.Select(mg => new
                    {
                        mg.MemberGoalId,
                        mg.UserId,
                        mg.GoalId,
                        mg.PersonalTarget,
                        mg.CurrentAmount,
                        mg.JoinedAt
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (goalData == null)
                throw new KeyNotFoundException("Goal not found");

            return new GoalResponse
            {
                GoalId = goalData.GoalId,
                GoalName = goalData.GoalName,
                Description = goalData.Description,
                TargetAmount = goalData.TargetAmount,
                CurrentAmount = goalData.CurrentAmount,
                ProgressPercentage = goalData.TargetAmount > 0 ? (goalData.CurrentAmount / goalData.TargetAmount) * 100 : 0,
                StartDate = goalData.StartDate,
                EndDate = goalData.EndDate,
                Status = goalData.Status,
                CreatedBy = goalData.CreatedBy,
                CreatedByName = goalData.CreatedByUser != null ? $"{goalData.CreatedByUser.FirstName} {goalData.CreatedByUser.LastName}" : "Unknown",
                CreatedAt = goalData.CreatedAt,
                MemberCount = goalData.MemberGoalsCount,
                IsOverdue = goalData.Status == "Active" && DateTime.UtcNow > goalData.EndDate,
                DaysRemaining = goalData.Status == "Active" ? (int)(goalData.EndDate - DateTime.UtcNow).TotalDays : 0
            };
        }

        public async Task<IEnumerable<GoalResponse>> GetAllGoalsAsync()
        {
            // Auto-update goals that have passed their end date
            var expiredGoals = await _context.SavingsGoals
                .AsNoTracking()
                .Where(g => g.Status == "Active" && DateTime.UtcNow > g.EndDate)
                .ToListAsync();

            if (expiredGoals.Any())
            {
                foreach (var goal in expiredGoals)
                {
                    goal.Status = "Completed"; // Use "Completed" instead of "Achieved" to match database constraint
                }
                _context.SavingsGoals.UpdateRange(expiredGoals);
                await _context.SaveChangesAsync();
            }

            // Use explicit projection to avoid any lazy loading or shadow property issues
            var goals = await _context.SavingsGoals
                .AsNoTracking()
                .Select(g => new
                {
                    g.GoalId,
                    g.GoalName,
                    g.Description,
                    g.TargetAmount,
                    g.CurrentAmount,
                    g.StartDate,
                    g.EndDate,
                    g.Status,
                    g.CreatedBy,
                    g.CreatedAt,
                    g.UpdatedAt,
                    CreatedByUser = g.CreatedByUser != null ? new
                    {
                        g.CreatedByUser.UserId,
                        g.CreatedByUser.FirstName,
                        g.CreatedByUser.LastName,
                        g.CreatedByUser.Email
                    } : null,
                    MemberGoalsCount = g.MemberGoals.Count,
                    MemberGoals = g.MemberGoals.Select(mg => new
                    {
                        mg.MemberGoalId,
                        mg.UserId,
                        mg.GoalId,
                        mg.PersonalTarget,
                        mg.CurrentAmount,
                        mg.JoinedAt
                    }).ToList()
                })
                .OrderByDescending(g => g.CreatedAt)
                .ToListAsync();

            // Convert to GoalResponse
            return goals.Select(g => new GoalResponse
            {
                GoalId = g.GoalId,
                GoalName = g.GoalName,
                Description = g.Description,
                TargetAmount = g.TargetAmount,
                CurrentAmount = g.CurrentAmount,
                ProgressPercentage = g.TargetAmount > 0 ? (g.CurrentAmount / g.TargetAmount) * 100 : 0,
                StartDate = g.StartDate,
                EndDate = g.EndDate,
                Status = g.Status,
                CreatedBy = g.CreatedBy,
                CreatedByName = g.CreatedByUser != null ? $"{g.CreatedByUser.FirstName} {g.CreatedByUser.LastName}" : "Unknown",
                CreatedAt = g.CreatedAt,
                MemberCount = g.MemberGoalsCount,
                IsOverdue = g.Status == "Active" && DateTime.UtcNow > g.EndDate,
                DaysRemaining = g.Status == "Active" ? (int)(g.EndDate - DateTime.UtcNow).TotalDays : 0
            });
        }

        public async Task<IEnumerable<GoalResponse>> GetGoalsByStatusAsync(string status)
        {
            var goals = await _context.SavingsGoals
                .Include(g => g.CreatedByUser)
                .Include(g => g.MemberGoals)
                .Where(g => g.Status == status)
                .OrderByDescending(g => g.CreatedAt)
                .ToListAsync();

            return goals.Select(MapToGoalResponse);
        }

        public async Task<IEnumerable<GoalResponse>> GetActiveGoalsAsync()
        {
            return await GetGoalsByStatusAsync("Active");
        }

        public async Task<GoalResponse> UpdateGoalAsync(int goalId, UpdateGoalRequest request)
        {
            var goal = await _context.SavingsGoals.FindAsync(goalId);
            if (goal == null)
                throw new KeyNotFoundException("Goal not found");

            if (!string.IsNullOrEmpty(request.GoalName))
                goal.GoalName = request.GoalName;

            if (request.Description != null)
                goal.Description = request.Description;

            if (request.TargetAmount.HasValue)
                goal.TargetAmount = request.TargetAmount.Value;

            if (request.EndDate.HasValue)
            {
                if (request.EndDate.Value <= goal.StartDate)
                    throw new ArgumentException("End date must be after start date");
                goal.EndDate = request.EndDate.Value;
            }

            if (!string.IsNullOrEmpty(request.Status))
                goal.Status = request.Status;

            _context.SavingsGoals.Update(goal);
            await _context.SaveChangesAsync();

            return await GetGoalByIdAsync(goalId);
        }

        public async Task<bool> DeleteGoalAsync(int goalId)
        {
            var goal = await _context.SavingsGoals
                .Include(g => g.Contributions)
                .Include(g => g.MemberGoals)
                .FirstOrDefaultAsync(g => g.GoalId == goalId);

            if (goal == null)
                throw new KeyNotFoundException("Goal not found");

            if (goal.Contributions.Any())
                throw new InvalidOperationException("Cannot delete goal with existing contributions");

            if (goal.MemberGoals.Any())
                _context.MemberGoals.RemoveRange(goal.MemberGoals);

            _context.SavingsGoals.Remove(goal);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<GoalStatsResponse> GetGoalStatsAsync()
        {
            var goals = await _context.SavingsGoals.ToListAsync();

            var totalGoals = goals.Count;
            var activeGoals = goals.Count(g => g.Status == "Active");
            var completedGoals = goals.Count(g => g.Status == "Completed");
            var overdueGoals = goals.Count(g => g.Status == "Active" && DateTime.Now > g.EndDate);
            var totalTargetAmount = goals.Sum(g => g.TargetAmount);
            var totalCurrentAmount = goals.Sum(g => g.CurrentAmount);
            var overallProgressPercentage = totalTargetAmount > 0 ? (totalCurrentAmount / totalTargetAmount) * 100 : 0;

            return new GoalStatsResponse
            {
                TotalGoals = totalGoals,
                ActiveGoals = activeGoals,
                CompletedGoals = completedGoals,
                OverdueGoals = overdueGoals,
                TotalTargetAmount = totalTargetAmount,
                TotalCurrentAmount = totalCurrentAmount,
                OverallProgressPercentage = overallProgressPercentage
            };
        }

        public async Task<MemberGoalResponse> JoinGoalAsync(int goalId, int userId, JoinGoalRequest request)
        {
            var goal = await _context.SavingsGoals.FindAsync(goalId);
            if (goal == null)
                throw new KeyNotFoundException("Goal not found");

            if (goal.Status != "Active")
                throw new InvalidOperationException("Cannot join inactive goal");

            var user = await _context.Users.FindAsync(userId);
            if (user == null || !user.IsActive)
                throw new KeyNotFoundException("User not found or inactive");

            var existingMemberGoal = await _context.MemberGoals
                .FirstOrDefaultAsync(mg => mg.UserId == userId && mg.GoalId == goalId);

            if (existingMemberGoal != null)
                throw new InvalidOperationException("User is already a member of this goal");

            var memberGoal = new MemberGoal
            {
                UserId = userId,
                GoalId = goalId,
                PersonalTarget = request.PersonalTarget ?? 0,
                CurrentAmount = 0,
                JoinedAt = DateTime.UtcNow
            };

            _context.MemberGoals.Add(memberGoal);
            await _context.SaveChangesAsync();

            return new MemberGoalResponse
            {
                MemberGoalId = memberGoal.MemberGoalId,
                UserId = memberGoal.UserId,
                UserName = $"{user.FirstName} {user.LastName}",
                GoalId = memberGoal.GoalId,
                GoalName = goal.GoalName,
                PersonalTarget = memberGoal.PersonalTarget,
                CurrentAmount = memberGoal.CurrentAmount,
                PersonalProgressPercentage = memberGoal.PersonalProgressPercentage,
                JoinedAt = memberGoal.JoinedAt
            };
        }

        public async Task<bool> LeaveGoalAsync(int goalId, int userId)
        {
            var memberGoal = await _context.MemberGoals
                .FirstOrDefaultAsync(mg => mg.UserId == userId && mg.GoalId == goalId);

            if (memberGoal == null)
                throw new KeyNotFoundException("User is not a member of this goal");

            _context.MemberGoals.Remove(memberGoal);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<MemberGoalResponse>> GetGoalMembersAsync(int goalId)
        {
            // Use projection to avoid loading ProfilePictureUrl column
            var memberGoals = await _context.MemberGoals
                .AsNoTracking()
                .Where(mg => mg.GoalId == goalId)
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
                .OrderByDescending(mg => mg.JoinedAt)
                .ToListAsync();

            return memberGoals;
        }

        public async Task<GoalResponse> UpdateGoalProgressAsync(int goalId)
        {
            var goal = await _context.SavingsGoals
                .Include(g => g.Contributions)
                .FirstOrDefaultAsync(g => g.GoalId == goalId);

            if (goal == null)
                throw new KeyNotFoundException("Goal not found");

            var approvedContributions = goal.Contributions
                .Where(c => c.Status == "Approved")
                .Sum(c => c.Amount);

            goal.CurrentAmount = approvedContributions;

            // Auto-update status based on progress
            if (goal.CurrentAmount >= goal.TargetAmount)
                goal.Status = "Completed";
            else if (DateTime.Now > goal.EndDate)
                goal.Status = goal.Status == "Active" ? "Cancelled" : goal.Status;

            _context.SavingsGoals.Update(goal);
            await _context.SaveChangesAsync();

            return await GetGoalByIdAsync(goalId);
        }

        private GoalResponse MapToGoalResponse(SavingsGoal goal)
        {
            return new GoalResponse
            {
                GoalId = goal.GoalId,
                GoalName = goal.GoalName,
                Description = goal.Description,
                TargetAmount = goal.TargetAmount,
                CurrentAmount = goal.CurrentAmount,
                ProgressPercentage = goal.ProgressPercentage,
                StartDate = goal.StartDate,
                EndDate = goal.EndDate,
                Status = goal.Status,
                CreatedBy = goal.CreatedBy,
                CreatedByName = goal.CreatedByUser?.FullName ?? "Unknown",
                CreatedAt = goal.CreatedAt,
                MemberCount = goal.MemberGoals.Count,
                IsOverdue = goal.IsOverdue,
                DaysRemaining = goal.Status == "Active" ? (int)(goal.EndDate - DateTime.Now).TotalDays : 0
            };
        }
    }
}