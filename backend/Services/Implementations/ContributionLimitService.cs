using CommunityFinanceAPI.Data;
using CommunityFinanceAPI.Models.DTOs;
using CommunityFinanceAPI.Models.Entities;
using CommunityFinanceAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CommunityFinanceAPI.Services.Implementations
{
    public class ContributionLimitService : IContributionLimitService
    {
        private readonly ApplicationDbContext _context;

        public ContributionLimitService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ContributionLimitResponse> CreateContributionLimitAsync(CreateContributionLimitRequest request)
        {
            // Check if goal exists
            var goal = await _context.SavingsGoals.FindAsync(request.GoalId);
            if (goal == null)
                throw new KeyNotFoundException("Goal not found");

            // Check if limit already exists for this goal
            var existingLimit = await _context.ContributionLimits
                .FirstOrDefaultAsync(cl => cl.GoalId == request.GoalId && cl.IsActive);
            
            if (existingLimit != null)
                throw new InvalidOperationException("An active contribution limit already exists for this goal. Please update the existing limit instead.");

            var limit = new ContributionLimit
            {
                GoalId = request.GoalId,
                FixedAmount = request.FixedAmount,
                MinimumAmount = request.MinimumAmount,
                MaximumAmount = request.MaximumAmount,
                MaximumTotalPerUser = request.MaximumTotalPerUser,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.ContributionLimits.Add(limit);
            await _context.SaveChangesAsync();

            return await MapToResponseAsync(limit);
        }

        public async Task<ContributionLimitResponse> UpdateContributionLimitAsync(int limitId, UpdateContributionLimitRequest request)
        {
            var limit = await _context.ContributionLimits.FindAsync(limitId);
            if (limit == null)
                throw new KeyNotFoundException("Contribution limit not found");

            if (request.FixedAmount.HasValue)
                limit.FixedAmount = request.FixedAmount;
            if (request.MinimumAmount.HasValue)
                limit.MinimumAmount = request.MinimumAmount;
            if (request.MaximumAmount.HasValue)
                limit.MaximumAmount = request.MaximumAmount;
            if (request.MaximumTotalPerUser.HasValue)
                limit.MaximumTotalPerUser = request.MaximumTotalPerUser;
            if (request.IsActive.HasValue)
                limit.IsActive = request.IsActive.Value;

            limit.UpdatedAt = DateTime.UtcNow;

            _context.ContributionLimits.Update(limit);
            await _context.SaveChangesAsync();

            return await MapToResponseAsync(limit);
        }

        public async Task<ContributionLimitResponse?> GetContributionLimitByGoalIdAsync(int goalId)
        {
            var limit = await _context.ContributionLimits
                .Include(cl => cl.Goal)
                .FirstOrDefaultAsync(cl => cl.GoalId == goalId && cl.IsActive);

            if (limit == null)
                return null;

            return await MapToResponseAsync(limit);
        }

        public async Task<IEnumerable<ContributionLimitResponse>> GetAllContributionLimitsAsync()
        {
            var limits = await _context.ContributionLimits
                .Include(cl => cl.Goal)
                .ToListAsync();

            var responses = new List<ContributionLimitResponse>();
            foreach (var limit in limits)
            {
                responses.Add(await MapToResponseAsync(limit));
            }

            return responses;
        }

        public async Task<bool> DeleteContributionLimitAsync(int limitId)
        {
            var limit = await _context.ContributionLimits.FindAsync(limitId);
            if (limit == null)
                throw new KeyNotFoundException("Contribution limit not found");

            _context.ContributionLimits.Remove(limit);
            await _context.SaveChangesAsync();

            return true;
        }

        private async Task<ContributionLimitResponse> MapToResponseAsync(ContributionLimit limit)
        {
            var goal = await _context.SavingsGoals.FindAsync(limit.GoalId);
            return new ContributionLimitResponse
            {
                LimitId = limit.LimitId,
                GoalId = limit.GoalId,
                GoalName = goal?.GoalName ?? "Unknown",
                FixedAmount = limit.FixedAmount,
                MinimumAmount = limit.MinimumAmount,
                MaximumAmount = limit.MaximumAmount,
                MaximumTotalPerUser = limit.MaximumTotalPerUser,
                IsActive = limit.IsActive,
                CreatedAt = limit.CreatedAt,
                UpdatedAt = limit.UpdatedAt
            };
        }
    }
}


