using CommunityFinanceAPI.Data;
using CommunityFinanceAPI.Models.DTOs;
using CommunityFinanceAPI.Models.Entities;
using CommunityFinanceAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CommunityFinanceAPI.Services.Implementations
{
    public class LoanService : ILoanService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private const decimal InterestRate = 5.0m; // 5%
        private const int LoanDurationDays = 30; // 1 month
        private const decimal MaxLoanPercentage = 12.5m; // 12.5% of total balance (fallback)

        // Contribution-based loan tiers (based on total contributions)
        private const decimal BronzeTierMin = 0m; // $0 - $499
        private const decimal SilverTierMin = 500m; // $500 - $1,999
        private const decimal GoldTierMin = 2000m; // $2,000 - $4,999
        private const decimal PlatinumTierMin = 5000m; // $5,000+

        // Regular contributor loan (for users who don't qualify for tiers or want simple calculation)
        private const decimal RegularContributorMultiplier = 1.0m; // Regular contributors can borrow 1x their contributions
        private const decimal RegularContributorMaxPercent = 10m; // 10% of total balance (default)

        // Loan multipliers based on contribution tier (multiplier of user's contributions)
        // These represent how much a user can borrow based on their contribution amount
        private const decimal BronzeLoanMultiplier = 1.0m; // Can borrow up to 1x their contributions
        private const decimal SilverLoanMultiplier = 1.5m; // Can borrow up to 1.5x their contributions
        private const decimal GoldLoanMultiplier = 2.0m; // Can borrow up to 2x their contributions
        private const decimal PlatinumLoanMultiplier = 3.0m; // Can borrow up to 3x their contributions

        // Max loan percentage based on contribution tier (percentage of total community balance)
        private const decimal BronzeMaxLoanPercent = 10m; // 10% of total balance
        private const decimal SilverMaxLoanPercent = 15m; // 15% of total balance
        private const decimal GoldMaxLoanPercent = 20m; // 20% of total balance
        private const decimal PlatinumMaxLoanPercent = 25m; // 25% of total balance

        public LoanService(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        /// <summary>
        /// Gets the user's contribution tier, loan multiplier, and max loan percentage based on their total approved contributions
        /// </summary>
        private async Task<(string Tier, decimal LoanMultiplier, decimal MaxLoanPercent)> GetUserContributionTierAsync(int userId)
        {
            var totalContributions = await _context.Contributions
                .Where(c => c.UserId == userId && c.Status == "Approved")
                .SumAsync(c => (decimal?)c.Amount) ?? 0m;

            if (totalContributions >= PlatinumTierMin)
                return ("Platinum", PlatinumLoanMultiplier, PlatinumMaxLoanPercent);
            else if (totalContributions >= GoldTierMin)
                return ("Gold", GoldLoanMultiplier, GoldMaxLoanPercent);
            else if (totalContributions >= SilverTierMin)
                return ("Silver", SilverLoanMultiplier, SilverMaxLoanPercent);
            else
                return ("Bronze", BronzeLoanMultiplier, BronzeMaxLoanPercent);
        }

        public async Task<LoanResponse> RequestLoanAsync(int userId, CreateLoanRequest request)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || !user.IsActive)
                throw new KeyNotFoundException("User not found or inactive");

            // Check if user has outstanding loans
            var hasOutstanding = await HasOutstandingLoansAsync(userId);
            if (hasOutstanding)
                throw new InvalidOperationException("Cannot request a loan while you have outstanding loans");

            // Get user's total contributions
            var userTotalContributions = await _context.Contributions
                .Where(c => c.UserId == userId && c.Status == "Approved")
                .SumAsync(c => (decimal?)c.Amount) ?? 0m;

            if (userTotalContributions <= 0)
                throw new InvalidOperationException("You must have made at least one approved contribution before requesting a loan.");

            // Get total account balance to ensure community has funds available
            var totalBalance = await GetTotalAccountBalanceAsync();
            if (totalBalance <= 0)
                throw new InvalidOperationException("No funds available for loans. The community fund balance is zero or negative.");

            // Get user's contribution tier to determine loan multiplier and percentage
            var (tier, loanMultiplier, maxLoanPercent) = await GetUserContributionTierAsync(userId);

            // Calculate max loan amount using BOTH methods and take the minimum (most restrictive)
            // Method 1: Based on user's contributions × tier multiplier (tier-based)
            var contributionBasedLimit = userTotalContributions * loanMultiplier;
            
            // Method 2: Based on percentage of total community balance (tier-based)
            var percentageBasedLimit = totalBalance * (maxLoanPercent / 100m);
            
            // Method 3: Regular contributor way (simple: 1x contributions or 10% of balance, whichever is lower)
            var regularContributorLimit = Math.Min(
                userTotalContributions * RegularContributorMultiplier,
                totalBalance * (RegularContributorMaxPercent / 100m)
            );
            
            // Use the minimum (most restrictive) of all three methods
            // This allows regular contributors to use the simpler calculation if it's more favorable
            var maxLoanAmount = Math.Min(Math.Min(contributionBasedLimit, percentageBasedLimit), regularContributorLimit);

            // Ensure the requested loan doesn't exceed what the community can provide
            // (user's max loan cannot exceed available community funds)
            var totalOutstandingLoans = await _context.Loans
                .Where(l => l.Status == "Approved" && l.RemainingAmount > 0)
                .SumAsync(l => l.RemainingAmount);

            var availableBalance = totalBalance - totalOutstandingLoans;
            if (maxLoanAmount > availableBalance)
                maxLoanAmount = availableBalance; // Cap at available community funds

            if (request.Amount > maxLoanAmount)
            {
                var method1 = $"{loanMultiplier}x contributions = {contributionBasedLimit:C}";
                var method2 = $"{maxLoanPercent}% of total balance = {percentageBasedLimit:C}";
                var usedMethod = maxLoanAmount == contributionBasedLimit ? method1 : method2;
                throw new ArgumentException($"Loan amount cannot exceed {maxLoanAmount:C}. Your contribution tier: {tier} (Total contributions: {userTotalContributions:C}). Calculated as: {usedMethod}, capped at available funds: {availableBalance:C}");
            }

            // Final check: ensure requested amount doesn't exceed available community funds
            if (request.Amount > availableBalance)
                throw new InvalidOperationException($"Insufficient community funds. Available: {availableBalance:C}");

            // Calculate interest and total
            var interest = request.Amount * (InterestRate / 100m);
            var totalAmount = request.Amount + interest;

            var loan = new Loan
            {
                UserId = userId,
                PrincipalAmount = request.Amount,
                InterestRate = InterestRate,
                TotalAmount = totalAmount,
                RemainingAmount = totalAmount,
                RequestedDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(LoanDurationDays),
                Status = "Pending"
            };

            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();

            return await GetLoanByIdAsync(loan.LoanId);
        }

        public async Task<LoanResponse> ApproveLoanAsync(int loanId, int approvedBy, ApproveLoanRequest? request = null)
        {
            var loan = await _context.Loans
                .Include(l => l.User)
                .FirstOrDefaultAsync(l => l.LoanId == loanId);

            if (loan == null)
                throw new KeyNotFoundException("Loan not found");

            if (loan.Status != "Pending")
                throw new InvalidOperationException("Only pending loans can be approved");

            // Verify funds are still available
            var totalBalance = await GetTotalAccountBalanceAsync();
            var totalOutstandingLoans = await _context.Loans
                .Where(l => l.Status == "Approved" && l.RemainingAmount > 0 && l.LoanId != loanId)
                .SumAsync(l => l.RemainingAmount);

            var availableBalance = totalBalance - totalOutstandingLoans;
            if (loan.PrincipalAmount > availableBalance)
                throw new InvalidOperationException("Insufficient funds to approve this loan");

            loan.Status = "Approved";
            loan.ApprovedBy = approvedBy;
            loan.ApprovedAt = DateTime.UtcNow;

            _context.Loans.Update(loan);
            await _context.SaveChangesAsync();

            return await GetLoanByIdAsync(loanId);
        }

        public async Task<LoanResponse> RejectLoanAsync(int loanId, int rejectedBy, RejectLoanRequest request)
        {
            var loan = await _context.Loans.FindAsync(loanId);
            if (loan == null)
                throw new KeyNotFoundException("Loan not found");

            if (loan.Status != "Pending")
                throw new InvalidOperationException("Only pending loans can be rejected");

            loan.Status = "Rejected";
            loan.ApprovedBy = rejectedBy;
            loan.ApprovedAt = DateTime.UtcNow;
            loan.RejectionReason = request.RejectionReason;

            _context.Loans.Update(loan);
            await _context.SaveChangesAsync();

            return await GetLoanByIdAsync(loanId);
        }

        public async Task<LoanResponse> PayLoanAsync(int loanId, int userId, PayLoanRequest request)
        {
            var loan = await _context.Loans
                .Include(l => l.User)
                .FirstOrDefaultAsync(l => l.LoanId == loanId);

            if (loan == null)
                throw new KeyNotFoundException("Loan not found");

            if (loan.UserId != userId)
                throw new UnauthorizedAccessException("You can only pay your own loans");

            if (loan.Status != "Approved")
                throw new InvalidOperationException("Only approved loans can be paid");

            if (request.Amount > loan.RemainingAmount)
                throw new ArgumentException("Payment amount cannot exceed remaining loan amount");

            var previousRemainingAmount = loan.RemainingAmount;
            loan.PaidAmount += request.Amount;
            loan.RemainingAmount -= request.Amount;

            // Create loan payment record for proof/history
            var loanPayment = new LoanPayment
            {
                LoanId = loanId,
                UserId = userId,
                Amount = request.Amount,
                PaymentReference = request.PaymentReference,
                PaymentDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.LoanPayments.Add(loanPayment);

            bool isFullyPaid = loan.RemainingAmount <= 0;
            if (isFullyPaid)
            {
                loan.Status = "Paid";
                // Distribute interest profit to all active members (including admin)
                await DistributeInterestProfitAsync(loan);
            }

            _context.Loans.Update(loan);
            await _context.SaveChangesAsync();

            // Send notifications
            await SendLoanPaymentNotificationsAsync(loan, request.Amount, isFullyPaid, previousRemainingAmount);

            return await GetLoanByIdAsync(loanId);
        }

        private async Task SendLoanPaymentNotificationsAsync(Loan loan, decimal paymentAmount, bool isFullyPaid, decimal previousRemainingAmount)
        {
            // Get the latest payment reference
            var latestPayment = await _context.LoanPayments
                .Where(lp => lp.LoanId == loan.LoanId)
                .OrderByDescending(lp => lp.PaymentDate)
                .FirstOrDefaultAsync();

            var paymentRef = latestPayment?.PaymentReference ?? "N/A";

            // Notify the member about their payment
            var memberMessage = isFullyPaid
                ? $"Your loan of {loan.PrincipalAmount:C} has been fully paid! Payment reference: {paymentRef}. Payment date: {latestPayment?.PaymentDate:MM/dd/yyyy}"
                : $"Payment of {paymentAmount:C} received. Remaining balance: {loan.RemainingAmount:C}. Payment reference: {paymentRef}. Payment date: {latestPayment?.PaymentDate:MM/dd/yyyy}";

            await _notificationService.CreateNotificationAsync(new CreateNotificationRequest
            {
                UserId = loan.UserId,
                Title = isFullyPaid ? "Loan Fully Paid" : "Loan Payment Received",
                Message = memberMessage,
                Type = "success",
                RelatedEntityType = "Loan",
                RelatedEntityId = loan.LoanId
            });

            // Notify all admins about the payment
            var admins = await _context.Users
                .Where(u => u.Role == "Admin" && u.IsActive)
                .ToListAsync();

            var adminMessage = isFullyPaid
                ? $"Member {loan.User.FirstName} {loan.User.LastName} has fully paid their loan of {loan.PrincipalAmount:C}. Payment reference: {paymentRef}. Payment date: {latestPayment?.PaymentDate:MM/dd/yyyy}"
                : $"Member {loan.User.FirstName} {loan.User.LastName} made a payment of {paymentAmount:C} on their loan. Remaining: {loan.RemainingAmount:C}. Payment reference: {paymentRef}. Payment date: {latestPayment?.PaymentDate:MM/dd/yyyy}";

            foreach (var admin in admins)
            {
                await _notificationService.CreateNotificationAsync(new CreateNotificationRequest
                {
                    UserId = admin.UserId,
                    Title = isFullyPaid ? "Loan Fully Paid by Member" : "Loan Payment Received",
                    Message = adminMessage,
                    Type = "info",
                    RelatedEntityType = "Loan",
                    RelatedEntityId = loan.LoanId
                });
            }
        }

        private async Task DistributeInterestProfitAsync(Loan loan)
        {
            // Get all active members (including admin)
            var activeMembers = await _context.Users
                .Where(u => u.IsActive)
                .ToListAsync();

            if (activeMembers.Count == 0) return;

            var interestAmount = loan.InterestRate > 0 ? loan.PrincipalAmount * (loan.InterestRate / 100m) : 0;
            var profitPerMember = interestAmount / activeMembers.Count;

            // Add profit as contributions to each member's account
            // We'll create a special "Interest Distribution" goal or track it separately
            // For now, we'll add it as contributions to a system goal
            // In a real system, you might want a separate profit distribution table

            // Note: This is a simplified implementation
            // In production, you'd want to track profit distributions separately
        }

        public async Task<LoanResponse> GetLoanByIdAsync(int loanId)
        {
            // Use projection to avoid Include() which might hit invalid shadow properties
            var loan = await _context.Loans
                .AsNoTracking()
                .Where(l => l.LoanId == loanId)
                .Select(l => new LoanResponse
                {
                    LoanId = l.LoanId,
                    UserId = l.UserId,
                    UserName = l.User != null ? $"{l.User.FirstName} {l.User.LastName}" : "Unknown",
                    PrincipalAmount = l.PrincipalAmount,
                    InterestRate = l.InterestRate,
                    TotalAmount = l.TotalAmount,
                    RemainingAmount = l.RemainingAmount,
                    PaidAmount = l.PaidAmount,
                    RequestedDate = l.RequestedDate,
                    DueDate = l.DueDate,
                    Status = l.Status,
                    ApprovedBy = l.ApprovedBy,
                    ApprovedByName = l.ApprovedByUser != null ? $"{l.ApprovedByUser.FirstName} {l.ApprovedByUser.LastName}" : null,
                    ApprovedAt = l.ApprovedAt,
                    RejectionReason = l.RejectionReason,
                    IsOverdue = l.Status == "Approved" && DateTime.UtcNow > l.DueDate && l.RemainingAmount > 0,
                    DaysUntilDue = l.Status == "Approved" ? (int)(l.DueDate - DateTime.UtcNow).TotalDays : 0,
                    CreatedAt = l.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (loan == null)
                throw new KeyNotFoundException("Loan not found");

            return loan;
        }

        public async Task<IEnumerable<LoanResponse>> GetUserLoansAsync(int userId)
        {
            // Use projection to avoid Include() which might hit invalid shadow properties
            var loans = await _context.Loans
                .AsNoTracking()
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.RequestedDate)
                .Select(l => new LoanResponse
                {
                    LoanId = l.LoanId,
                    UserId = l.UserId,
                    UserName = l.User != null ? $"{l.User.FirstName} {l.User.LastName}" : "Unknown",
                    PrincipalAmount = l.PrincipalAmount,
                    InterestRate = l.InterestRate,
                    TotalAmount = l.TotalAmount,
                    RemainingAmount = l.RemainingAmount,
                    PaidAmount = l.PaidAmount,
                    RequestedDate = l.RequestedDate,
                    DueDate = l.DueDate,
                    Status = l.Status,
                    ApprovedBy = l.ApprovedBy,
                    ApprovedByName = l.ApprovedByUser != null ? $"{l.ApprovedByUser.FirstName} {l.ApprovedByUser.LastName}" : null,
                    ApprovedAt = l.ApprovedAt,
                    RejectionReason = l.RejectionReason,
                    IsOverdue = l.Status == "Approved" && DateTime.UtcNow > l.DueDate && l.RemainingAmount > 0,
                    DaysUntilDue = l.Status == "Approved" ? (int)(l.DueDate - DateTime.UtcNow).TotalDays : 0,
                    CreatedAt = l.CreatedAt
                })
                .ToListAsync();

            return loans;
        }

        public async Task<IEnumerable<LoanResponse>> GetPendingLoansAsync()
        {
            // Use projection to avoid Include() which might hit invalid shadow properties
            var loans = await _context.Loans
                .AsNoTracking()
                .Where(l => l.Status == "Pending")
                .OrderBy(l => l.RequestedDate)
                .Select(l => new LoanResponse
                {
                    LoanId = l.LoanId,
                    UserId = l.UserId,
                    UserName = l.User != null ? $"{l.User.FirstName} {l.User.LastName}" : "Unknown",
                    PrincipalAmount = l.PrincipalAmount,
                    InterestRate = l.InterestRate,
                    TotalAmount = l.TotalAmount,
                    RemainingAmount = l.RemainingAmount,
                    PaidAmount = l.PaidAmount,
                    RequestedDate = l.RequestedDate,
                    DueDate = l.DueDate,
                    Status = l.Status,
                    ApprovedBy = l.ApprovedBy,
                    ApprovedByName = l.ApprovedByUser != null ? $"{l.ApprovedByUser.FirstName} {l.ApprovedByUser.LastName}" : null,
                    ApprovedAt = l.ApprovedAt,
                    RejectionReason = l.RejectionReason,
                    IsOverdue = l.Status == "Approved" && DateTime.UtcNow > l.DueDate && l.RemainingAmount > 0,
                    DaysUntilDue = l.Status == "Approved" ? (int)(l.DueDate - DateTime.UtcNow).TotalDays : 0,
                    CreatedAt = l.CreatedAt
                })
                .ToListAsync();

            return loans;
        }

        public async Task<IEnumerable<LoanResponse>> GetActiveLoansAsync()
        {
            // Use projection to avoid Include() which might hit invalid shadow properties
            var loans = await _context.Loans
                .AsNoTracking()
                .Where(l => l.Status == "Approved" && l.RemainingAmount > 0)
                .OrderBy(l => l.DueDate)
                .Select(l => new LoanResponse
                {
                    LoanId = l.LoanId,
                    UserId = l.UserId,
                    UserName = l.User != null ? $"{l.User.FirstName} {l.User.LastName}" : "Unknown",
                    PrincipalAmount = l.PrincipalAmount,
                    InterestRate = l.InterestRate,
                    TotalAmount = l.TotalAmount,
                    RemainingAmount = l.RemainingAmount,
                    PaidAmount = l.PaidAmount,
                    RequestedDate = l.RequestedDate,
                    DueDate = l.DueDate,
                    Status = l.Status,
                    ApprovedBy = l.ApprovedBy,
                    ApprovedByName = l.ApprovedByUser != null ? $"{l.ApprovedByUser.FirstName} {l.ApprovedByUser.LastName}" : null,
                    ApprovedAt = l.ApprovedAt,
                    RejectionReason = l.RejectionReason,
                    IsOverdue = l.Status == "Approved" && DateTime.UtcNow > l.DueDate && l.RemainingAmount > 0,
                    DaysUntilDue = l.Status == "Approved" ? (int)(l.DueDate - DateTime.UtcNow).TotalDays : 0,
                    CreatedAt = l.CreatedAt
                })
                .ToListAsync();

            return loans;
        }

        public async Task<IEnumerable<LoanResponse>> GetAllLoansAsync()
        {
            // Use projection to avoid Include() which might hit invalid shadow properties
            var loans = await _context.Loans
                .AsNoTracking()
                .OrderByDescending(l => l.RequestedDate)
                .Select(l => new LoanResponse
                {
                    LoanId = l.LoanId,
                    UserId = l.UserId,
                    UserName = l.User != null ? $"{l.User.FirstName} {l.User.LastName}" : "Unknown",
                    PrincipalAmount = l.PrincipalAmount,
                    InterestRate = l.InterestRate,
                    TotalAmount = l.TotalAmount,
                    RemainingAmount = l.RemainingAmount,
                    PaidAmount = l.PaidAmount,
                    RequestedDate = l.RequestedDate,
                    DueDate = l.DueDate,
                    Status = l.Status,
                    ApprovedBy = l.ApprovedBy,
                    ApprovedByName = l.ApprovedByUser != null ? $"{l.ApprovedByUser.FirstName} {l.ApprovedByUser.LastName}" : null,
                    ApprovedAt = l.ApprovedAt,
                    RejectionReason = l.RejectionReason,
                    IsOverdue = l.Status == "Approved" && DateTime.UtcNow > l.DueDate && l.RemainingAmount > 0,
                    DaysUntilDue = l.Status == "Approved" ? (int)(l.DueDate - DateTime.UtcNow).TotalDays : 0,
                    CreatedAt = l.CreatedAt
                })
                .ToListAsync();

            return loans;
        }

        public async Task<MemberAccountResponse> GetMemberAccountAsync(int userId)
        {
            // Optimize: Do all calculations in the database instead of loading all loans into memory
            var user = await _context.Users
                .AsNoTracking()
                .Where(u => u.UserId == userId)
                .Select(u => new
                {
                    u.UserId,
                    u.FirstName,
                    u.LastName,
                    AccountBalance = u.Contributions
                        .Where(c => c.Status == "Approved")
                        .Sum(c => (decimal?)c.Amount) ?? 0m,
                    TotalLoansTaken = u.Loans.Sum(l => (decimal?)l.PrincipalAmount) ?? 0m,
                    TotalLoansPaid = u.Loans.Where(l => l.Status == "Paid").Sum(l => (decimal?)l.PrincipalAmount) ?? 0m,
                    OutstandingLoanAmount = u.Loans
                        .Where(l => l.Status == "Approved" && l.RemainingAmount > 0)
                        .Sum(l => (decimal?)l.RemainingAmount) ?? 0m,
                    ActiveLoansCount = u.Loans.Count(l => l.Status == "Approved" && l.RemainingAmount > 0),
                    OverdueLoansCount = u.Loans.Count(l => l.Status == "Approved" && l.RemainingAmount > 0 && DateTime.UtcNow > l.DueDate)
                })
                .FirstOrDefaultAsync();

            if (user == null)
                throw new KeyNotFoundException("User not found");

            // Only load active loans for the response (much smaller dataset)
            var activeLoans = await _context.Loans
                .AsNoTracking()
                .Where(l => l.UserId == userId && l.Status == "Approved" && l.RemainingAmount > 0)
                .OrderByDescending(l => l.RequestedDate)
                .Select(l => new LoanResponse
                {
                    LoanId = l.LoanId,
                    UserId = l.UserId,
                    UserName = $"{user.FirstName} {user.LastName}",
                    PrincipalAmount = l.PrincipalAmount,
                    InterestRate = l.InterestRate,
                    TotalAmount = l.TotalAmount,
                    RemainingAmount = l.RemainingAmount,
                    PaidAmount = l.PaidAmount,
                    RequestedDate = l.RequestedDate,
                    DueDate = l.DueDate,
                    Status = l.Status,
                    ApprovedBy = l.ApprovedBy,
                    ApprovedByName = l.ApprovedByUser != null ? $"{l.ApprovedByUser.FirstName} {l.ApprovedByUser.LastName}" : null,
                    ApprovedAt = l.ApprovedAt,
                    RejectionReason = l.RejectionReason,
                    IsOverdue = DateTime.UtcNow > l.DueDate,
                    DaysUntilDue = (int)(l.DueDate - DateTime.UtcNow).TotalDays,
                    CreatedAt = l.CreatedAt
                })
                .ToListAsync();

            // Get user's contribution tier and calculate max loan using all methods
            var (tier, loanMultiplier, maxLoanPercent) = await GetUserContributionTierAsync(userId);
            
            // Method 1: Based on user's contributions × tier multiplier
            var contributionBasedLimit = user.AccountBalance * loanMultiplier;
            
            // Method 2: Based on percentage of total community balance
            var totalBalance = await GetTotalAccountBalanceAsync();
            var percentageBasedLimit = totalBalance * (maxLoanPercent / 100m);
            
            // Method 3: Regular contributor way (simple: 1x contributions or 10% of balance)
            var regularContributorLimit = Math.Min(
                user.AccountBalance * RegularContributorMultiplier,
                totalBalance * (RegularContributorMaxPercent / 100m)
            );
            
            // Use the minimum (most restrictive) of all three methods
            var maxLoanAmount = Math.Min(Math.Min(contributionBasedLimit, percentageBasedLimit), regularContributorLimit);

            // Cap at available community funds
            var totalOutstandingLoans = await _context.Loans
                .Where(l => l.Status == "Approved" && l.RemainingAmount > 0)
                .SumAsync(l => (decimal?)l.RemainingAmount) ?? 0m;
            var availableBalance = totalBalance - totalOutstandingLoans;
            if (maxLoanAmount > availableBalance)
                maxLoanAmount = availableBalance;

            return new MemberAccountResponse
            {
                UserId = user.UserId,
                UserName = $"{user.FirstName} {user.LastName}",
                AccountBalance = user.AccountBalance,
                TotalLoansTaken = user.TotalLoansTaken,
                TotalLoansPaid = user.TotalLoansPaid,
                OutstandingLoanAmount = user.OutstandingLoanAmount,
                ActiveLoansCount = user.ActiveLoansCount,
                OverdueLoansCount = user.OverdueLoansCount,
                ActiveLoans = activeLoans,
                ContributionTier = tier,
                MaxLoanAmount = maxLoanAmount,
                MaxLoanPercentage = maxLoanPercent // Percentage of total balance for this tier
            };
        }

        public async Task<LoanStatsResponse> GetLoanStatsAsync()
        {
            var totalBalance = await GetTotalAccountBalanceAsync();
            var loans = await _context.Loans.ToListAsync();

            var totalOutstanding = loans
                .Where(l => l.Status == "Approved" && l.RemainingAmount > 0)
                .Sum(l => l.RemainingAmount);

            var totalPaid = loans
                .Where(l => l.Status == "Paid")
                .Sum(l => l.PrincipalAmount);

            var totalInterest = loans
                .Where(l => l.Status == "Paid")
                .Sum(l => l.PrincipalAmount * (l.InterestRate / 100m));

            var pendingCount = loans.Count(l => l.Status == "Pending");
            var activeCount = loans.Count(l => l.Status == "Approved" && l.RemainingAmount > 0);
            var overdueCount = loans.Count(l => l.Status == "Approved" && l.RemainingAmount > 0 && l.IsOverdue);

            var maxIndividualLoan = totalBalance * (MaxLoanPercentage / 100m);

            return new LoanStatsResponse
            {
                TotalAccountBalance = totalBalance,
                TotalLoansOutstanding = totalOutstanding,
                TotalLoansPaid = totalPaid,
                TotalInterestEarned = totalInterest,
                PendingLoanRequests = pendingCount,
                ActiveLoans = activeCount,
                OverdueLoans = overdueCount,
                MaxIndividualLoanAmount = maxIndividualLoan
            };
        }

        public async Task<decimal> GetTotalAccountBalanceAsync()
        {
            // Total balance = sum of all approved contributions
            var total = await _context.Contributions
                .Where(c => c.Status == "Approved")
                .SumAsync(c => (decimal?)c.Amount);
            
            return total ?? 0m;
        }

        public async Task<bool> HasOutstandingLoansAsync(int userId)
        {
            return await _context.Loans
                .AnyAsync(l => l.UserId == userId && 
                              l.Status == "Approved" && 
                              l.RemainingAmount > 0);
        }

        public async Task<bool> HasTakenLoanThisYearAsync(int userId)
        {
            var currentYear = DateTime.UtcNow.Year;
            return await _context.Loans
                .AnyAsync(l => l.UserId == userId && 
                              l.RequestedDate.Year == currentYear &&
                              (l.Status == "Approved" || l.Status == "Paid"));
        }

        private LoanResponse MapToLoanResponse(Loan loan)
        {
            return new LoanResponse
            {
                LoanId = loan.LoanId,
                UserId = loan.UserId,
                UserName = loan.User?.FullName ?? "Unknown",
                PrincipalAmount = loan.PrincipalAmount,
                InterestRate = loan.InterestRate,
                TotalAmount = loan.TotalAmount,
                RemainingAmount = loan.RemainingAmount,
                PaidAmount = loan.PaidAmount,
                RequestedDate = loan.RequestedDate,
                DueDate = loan.DueDate,
                Status = loan.Status,
                ApprovedBy = loan.ApprovedBy,
                ApprovedByName = loan.ApprovedByUser?.FullName,
                ApprovedAt = loan.ApprovedAt,
                RejectionReason = loan.RejectionReason,
                IsOverdue = loan.IsOverdue,
                DaysUntilDue = loan.DaysUntilDue,
                CreatedAt = loan.CreatedAt
            };
        }
    }
}

