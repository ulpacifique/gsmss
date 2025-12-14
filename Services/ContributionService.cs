using CommunityFinanceAPI.Data;
using CommunityFinanceAPI.Models.DTOs;
using CommunityFinanceAPI.Models.Entities;
using CommunityFinanceAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CommunityFinanceAPI.Services.Implementations
{
    public class ContributionService : IContributionService
    {
        private readonly ApplicationDbContext _context;
        private readonly IGoalService _goalService;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public ContributionService(ApplicationDbContext context, IGoalService goalService, IServiceScopeFactory serviceScopeFactory)
        {
            _context = context;
            _goalService = goalService;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task<ContributionResponse> CreateContributionAsync(CreateContributionRequest request, int userId)
        {
            var goal = await _context.SavingsGoals.FindAsync(request.GoalId);
            if (goal == null || goal.Status != "Active")
                throw new InvalidOperationException("Goal not found or not active");

            var user = await _context.Users.FindAsync(userId);
            if (user == null || !user.IsActive)
                throw new InvalidOperationException("User not found or inactive");

            // Check if user is a member of the goal
            var isMember = await _context.MemberGoals
                .AnyAsync(mg => mg.UserId == userId && mg.GoalId == request.GoalId);

            if (!isMember)
            {
                // Auto-join the user to the goal if they're not a member
                var memberGoal = new Models.Entities.MemberGoal
                {
                    UserId = userId,
                    GoalId = request.GoalId,
                    JoinedAt = DateTime.UtcNow
                };
                _context.MemberGoals.Add(memberGoal);
                await _context.SaveChangesAsync();
            }

            // Use member's full name as payment reference if not provided or empty
            var paymentReference = !string.IsNullOrWhiteSpace(request.PaymentReference) 
                ? request.PaymentReference 
                : $"{user.FirstName} {user.LastName}";
            
            // Ensure PaymentReference doesn't exceed max length (100)
            if (paymentReference.Length > 100)
            {
                paymentReference = paymentReference.Substring(0, 100);
            }

            var now = DateTime.UtcNow;
            var contribution = new Contribution
            {
                UserId = userId,
                GoalId = request.GoalId,
                Amount = request.Amount,
                PaymentReference = paymentReference ?? string.Empty,
                Status = "Pending",
                SubmittedAt = now,
                CreatedAt = now, // Explicitly set to avoid database constraint issues
                UpdatedAt = now // Explicitly set to avoid database constraint issues
            };

            _context.Contributions.Add(contribution);
            
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                // Log the full exception details
                var innerException = dbEx.InnerException;
                var errorMessage = innerException?.Message ?? dbEx.Message;
                
                // Try to extract SQL error details if available
                if (innerException is Microsoft.Data.SqlClient.SqlException sqlEx)
                {
                    errorMessage = $"SQL Error {sqlEx.Number}: {sqlEx.Message}";
                    if (sqlEx.Errors.Count > 0)
                    {
                        var sqlError = sqlEx.Errors[0];
                        errorMessage = $"SQL Error {sqlError.Number} (State {sqlError.State}): {sqlError.Message}";
                    }
                }
                
                throw new InvalidOperationException($"Database error while saving contribution: {errorMessage}", dbEx);
            }

            return await GetContributionByIdAsync(contribution.ContributionId);
        }

        public async Task<ContributionResponse> GetContributionByIdAsync(int contributionId)
        {
            // Use projection to avoid Include() which might hit invalid shadow properties
            var contribution = await _context.Contributions
                .AsNoTracking()
                .Where(c => c.ContributionId == contributionId)
                .Select(c => new
                {
                    c.ContributionId,
                    c.UserId,
                    c.GoalId,
                    c.Amount,
                    c.PaymentReference,
                    c.Status,
                    c.SubmittedAt,
                    c.ReviewedBy,
                    c.ReviewedAt,
                    c.RejectionReason,
                    c.CreatedAt,
                    c.UpdatedAt,
                    User = c.User != null ? new
                    {
                        c.User.UserId,
                        c.User.FirstName,
                        c.User.LastName,
                        c.User.Email
                    } : null,
                    Goal = c.Goal != null ? new
                    {
                        c.Goal.GoalId,
                        c.Goal.GoalName
                    } : null,
                    ReviewedByUser = c.ReviewedByUser != null ? new
                    {
                        c.ReviewedByUser.UserId,
                        c.ReviewedByUser.FirstName,
                        c.ReviewedByUser.LastName,
                        c.ReviewedByUser.Email
                    } : null
                })
                .FirstOrDefaultAsync();

            if (contribution == null)
                throw new KeyNotFoundException("Contribution not found");

            return new ContributionResponse
            {
                ContributionId = contribution.ContributionId,
                UserId = contribution.UserId,
                UserName = contribution.User != null ? $"{contribution.User.FirstName} {contribution.User.LastName}" : "Unknown",
                GoalId = contribution.GoalId,
                GoalName = contribution.Goal?.GoalName ?? "Unknown",
                Amount = contribution.Amount,
                PaymentReference = contribution.PaymentReference,
                Status = contribution.Status,
                SubmittedAt = contribution.SubmittedAt,
                ReviewedBy = contribution.ReviewedBy,
                ReviewedByName = contribution.ReviewedByUser != null ? $"{contribution.ReviewedByUser.FirstName} {contribution.ReviewedByUser.LastName}" : null,
                ReviewedAt = contribution.ReviewedAt,
                RejectionReason = contribution.RejectionReason,
                CreatedAt = contribution.CreatedAt
            };
        }

        public async Task<IEnumerable<ContributionResponse>> GetAllContributionsAsync()
        {
            // Use projection to avoid Include() which might hit invalid shadow properties
            var contributions = await _context.Contributions
                .AsNoTracking()
                .OrderByDescending(c => c.SubmittedAt)
                .Select(c => new ContributionResponse
                {
                    ContributionId = c.ContributionId,
                    UserId = c.UserId,
                    UserName = c.User != null ? $"{c.User.FirstName} {c.User.LastName}" : "Unknown",
                    GoalId = c.GoalId,
                    GoalName = c.Goal != null ? c.Goal.GoalName : "Unknown",
                    Amount = c.Amount,
                    PaymentReference = c.PaymentReference,
                    Status = c.Status,
                    SubmittedAt = c.SubmittedAt,
                    ReviewedBy = c.ReviewedBy,
                    ReviewedByName = c.ReviewedByUser != null ? $"{c.ReviewedByUser.FirstName} {c.ReviewedByUser.LastName}" : null,
                    ReviewedAt = c.ReviewedAt,
                    RejectionReason = c.RejectionReason,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return contributions;
        }

        public async Task<IEnumerable<ContributionResponse>> GetContributionsByUserAsync(int userId)
        {
            // Use projection to avoid Include() which might hit invalid shadow properties
            var contributions = await _context.Contributions
                .AsNoTracking()
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.SubmittedAt)
                .Select(c => new ContributionResponse
                {
                    ContributionId = c.ContributionId,
                    UserId = c.UserId,
                    UserName = c.User != null ? $"{c.User.FirstName} {c.User.LastName}" : "Unknown",
                    GoalId = c.GoalId,
                    GoalName = c.Goal != null ? c.Goal.GoalName : "Unknown",
                    Amount = c.Amount,
                    PaymentReference = c.PaymentReference,
                    Status = c.Status,
                    SubmittedAt = c.SubmittedAt,
                    ReviewedBy = c.ReviewedBy,
                    ReviewedByName = c.ReviewedByUser != null ? $"{c.ReviewedByUser.FirstName} {c.ReviewedByUser.LastName}" : null,
                    ReviewedAt = c.ReviewedAt,
                    RejectionReason = c.RejectionReason,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return contributions;
        }

        public async Task<IEnumerable<ContributionResponse>> GetContributionsByGoalAsync(int goalId)
        {
            // Use projection to avoid Include() which might hit invalid shadow properties
            var contributions = await _context.Contributions
                .AsNoTracking()
                .Where(c => c.GoalId == goalId)
                .OrderByDescending(c => c.SubmittedAt)
                .Select(c => new ContributionResponse
                {
                    ContributionId = c.ContributionId,
                    UserId = c.UserId,
                    UserName = c.User != null ? $"{c.User.FirstName} {c.User.LastName}" : "Unknown",
                    GoalId = c.GoalId,
                    GoalName = c.Goal != null ? c.Goal.GoalName : "Unknown",
                    Amount = c.Amount,
                    PaymentReference = c.PaymentReference,
                    Status = c.Status,
                    SubmittedAt = c.SubmittedAt,
                    ReviewedBy = c.ReviewedBy,
                    ReviewedByName = c.ReviewedByUser != null ? $"{c.ReviewedByUser.FirstName} {c.ReviewedByUser.LastName}" : null,
                    ReviewedAt = c.ReviewedAt,
                    RejectionReason = c.RejectionReason,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return contributions;
        }

        public async Task<IEnumerable<ContributionResponse>> GetContributionsByStatusAsync(string status)
        {
            // Use projection only, no Include() to avoid shadow properties
            return await _context.Contributions
                .AsNoTracking()
                .Where(c => c.Status == status)
                .OrderByDescending(c => c.SubmittedAt)
                .Select(c => new ContributionResponse
                {
                    ContributionId = c.ContributionId,
                    UserId = c.UserId,
                    UserName = c.User != null ? $"{c.User.FirstName} {c.User.LastName}" : "Unknown",
                    GoalId = c.GoalId,
                    GoalName = c.Goal != null ? c.Goal.GoalName : "Unknown",
                    Amount = c.Amount,
                    PaymentReference = c.PaymentReference,
                    Status = c.Status,
                    SubmittedAt = c.SubmittedAt,
                    ReviewedBy = c.ReviewedBy,
                    ReviewedByName = c.ReviewedByUser != null ? $"{c.ReviewedByUser.FirstName} {c.ReviewedByUser.LastName}" : null,
                    ReviewedAt = c.ReviewedAt,
                    RejectionReason = c.RejectionReason,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<ContributionResponse>> GetPendingContributionsAsync()
        {
            return await GetContributionsByStatusAsync("Pending");
        }

        public async Task<ContributionResponse> UpdateContributionStatusAsync(int contributionId, UpdateContributionStatusRequest request, int reviewedBy)
        {
            // Load contribution without Include to avoid shadow properties
            var contribution = await _context.Contributions
                .FirstOrDefaultAsync(c => c.ContributionId == contributionId);

            if (contribution == null)
                throw new KeyNotFoundException("Contribution not found");

            if (contribution.Status != "Pending")
                throw new InvalidOperationException("Only pending contributions can be updated");

            contribution.Status = request.Status;
            contribution.ReviewedBy = reviewedBy;
            contribution.ReviewedAt = DateTime.UtcNow;
            contribution.RejectionReason = request.RejectionReason;

            if (request.Status == "Approved")
            {
                // Load goal separately to avoid Include
                var goal = await _context.SavingsGoals.FindAsync(contribution.GoalId);
                if (goal != null)
                {
                    goal.CurrentAmount += contribution.Amount;
                    _context.SavingsGoals.Update(goal);
                }

                // Update member goal current amount
                var memberGoal = await _context.MemberGoals
                    .FirstOrDefaultAsync(mg => mg.UserId == contribution.UserId && mg.GoalId == contribution.GoalId);

                if (memberGoal != null)
                {
                    memberGoal.CurrentAmount += contribution.Amount;
                    _context.MemberGoals.Update(memberGoal);
                }

                // Auto-update goal progress
                await _goalService.UpdateGoalProgressAsync(contribution.GoalId);
            }

            _context.Contributions.Update(contribution);
            await _context.SaveChangesAsync();

            // Create notification for the user
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                
                // Load goal name separately
                var goal = await _context.SavingsGoals.FindAsync(contribution.GoalId);
                var goalName = goal?.GoalName ?? "Unknown Goal";
                
                var notificationTitle = request.Status == "Approved" 
                    ? "Contribution Approved" 
                    : "Contribution Rejected";
                var notificationMessage = request.Status == "Approved"
                    ? $"Your contribution of ${contribution.Amount:N2} for goal '{goalName}' has been approved."
                    : $"Your contribution of ${contribution.Amount:N2} has been rejected.{(string.IsNullOrWhiteSpace(request.RejectionReason) ? "" : $" Reason: {request.RejectionReason}")}";

                await notificationService.CreateNotificationAsync(new Models.DTOs.CreateNotificationRequest
                {
                    UserId = contribution.UserId,
                    Title = notificationTitle,
                    Message = notificationMessage,
                    Type = request.Status == "Approved" ? "success" : "error",
                    RelatedEntityType = "Contribution",
                    RelatedEntityId = contributionId
                });
            }
            catch (Exception ex)
            {
                // Log but don't fail the contribution update if notification fails
                // Notification service might not be available yet
            }

            return await GetContributionByIdAsync(contributionId);
        }

        public async Task<bool> DeleteContributionAsync(int contributionId)
        {
            var contribution = await _context.Contributions.FindAsync(contributionId);
            if (contribution == null)
                throw new KeyNotFoundException("Contribution not found");

            if (contribution.Status == "Approved")
                throw new InvalidOperationException("Cannot delete approved contribution");

            _context.Contributions.Remove(contribution);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<ContributionStatsResponse> GetContributionStatsAsync()
        {
            // Use AsNoTracking and projection to avoid hitting invalid shadow properties
            var contributions = await _context.Contributions
                .AsNoTracking()
                .Select(c => new
                {
                    c.Status,
                    c.Amount,
                    c.CreatedAt
                })
                .ToListAsync();

            var totalContributions = contributions.Count;
            var pendingContributions = contributions.Count(c => c.Status == "Pending");
            var approvedContributions = contributions.Count(c => c.Status == "Approved");
            var rejectedContributions = contributions.Count(c => c.Status == "Rejected");
            var totalAmount = contributions.Where(c => c.Status == "Approved").Sum(c => c.Amount);
            var averageContribution = approvedContributions > 0 ? totalAmount / approvedContributions : 0;

            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;
            var lastMonth = currentMonth == 1 ? 12 : currentMonth - 1;
            var lastMonthYear = currentMonth == 1 ? currentYear - 1 : currentYear;

            var thisMonthTotal = contributions
                .Where(c => c.Status == "Approved" &&
                           c.CreatedAt.Month == currentMonth &&
                           c.CreatedAt.Year == currentYear)
                .Sum(c => c.Amount);

            var lastMonthTotal = contributions
                .Where(c => c.Status == "Approved" &&
                           c.CreatedAt.Month == lastMonth &&
                           c.CreatedAt.Year == lastMonthYear)
                .Sum(c => c.Amount);

            var percentageChange = lastMonthTotal > 0
                ? ((thisMonthTotal - lastMonthTotal) / lastMonthTotal) * 100
                : (thisMonthTotal > 0 ? 100 : 0);

            return new ContributionStatsResponse
            {
                TotalContributions = totalContributions,
                PendingContributions = pendingContributions,
                ApprovedContributions = approvedContributions,
                RejectedContributions = rejectedContributions,
                TotalAmount = totalAmount,
                AverageContribution = averageContribution,
                ThisMonthTotal = thisMonthTotal,
                LastMonthTotal = lastMonthTotal,
                PercentageChange = percentageChange
            };
        }

        public async Task<ContributionStatsResponse> GetUserContributionStatsAsync(int userId)
        {
            var contributions = await _context.Contributions
                .Where(c => c.UserId == userId)
                .ToListAsync();

            var totalContributions = contributions.Count;
            var pendingContributions = contributions.Count(c => c.Status == "Pending");
            var approvedContributions = contributions.Count(c => c.Status == "Approved");
            var rejectedContributions = contributions.Count(c => c.Status == "Rejected");
            var totalAmount = contributions.Where(c => c.Status == "Approved").Sum(c => c.Amount);
            var averageContribution = approvedContributions > 0 ? totalAmount / approvedContributions : 0;

            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;
            var lastMonth = currentMonth == 1 ? 12 : currentMonth - 1;
            var lastMonthYear = currentMonth == 1 ? currentYear - 1 : currentYear;

            var thisMonthTotal = contributions
                .Where(c => c.Status == "Approved" &&
                           c.CreatedAt.Month == currentMonth &&
                           c.CreatedAt.Year == currentYear)
                .Sum(c => c.Amount);

            var lastMonthTotal = contributions
                .Where(c => c.Status == "Approved" &&
                           c.CreatedAt.Month == lastMonth &&
                           c.CreatedAt.Year == lastMonthYear)
                .Sum(c => c.Amount);

            var percentageChange = lastMonthTotal > 0
                ? ((thisMonthTotal - lastMonthTotal) / lastMonthTotal) * 100
                : (thisMonthTotal > 0 ? 100 : 0);

            return new ContributionStatsResponse
            {
                TotalContributions = totalContributions,
                PendingContributions = pendingContributions,
                ApprovedContributions = approvedContributions,
                RejectedContributions = rejectedContributions,
                TotalAmount = totalAmount,
                AverageContribution = averageContribution,
                ThisMonthTotal = thisMonthTotal,
                LastMonthTotal = lastMonthTotal,
                PercentageChange = percentageChange
            };
        }

        private ContributionResponse MapToContributionResponse(Contribution contribution)
        {
            return new ContributionResponse
            {
                ContributionId = contribution.ContributionId,
                UserId = contribution.UserId,
                UserName = contribution.User?.FullName ?? "Unknown",
                GoalId = contribution.GoalId,
                GoalName = contribution.Goal?.GoalName ?? "Unknown",
                Amount = contribution.Amount,
                PaymentReference = contribution.PaymentReference,
                Status = contribution.Status,
                SubmittedAt = contribution.SubmittedAt,
                ReviewedBy = contribution.ReviewedBy,
                ReviewedByName = contribution.ReviewedByUser?.FullName,
                ReviewedAt = contribution.ReviewedAt,
                RejectionReason = contribution.RejectionReason,
                CreatedAt = contribution.CreatedAt
            };
        }
    }
}