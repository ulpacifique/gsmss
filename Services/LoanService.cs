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
        private const decimal InterestRate = 5.0m; // 5%
        private const int LoanDurationDays = 30; // 1 month
        private const decimal MaxLoanPercentage = 12.5m; // 12.5% of total balance

        public LoanService(ApplicationDbContext context)
        {
            _context = context;
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

            // Get total account balance (sum of all approved contributions)
            var totalBalance = await GetTotalAccountBalanceAsync();
            if (totalBalance <= 0)
                throw new InvalidOperationException("No funds available for loans. The community fund balance is zero or negative.");

            // Calculate max loan amount (12.5% of total balance)
            var maxLoanAmount = totalBalance * (MaxLoanPercentage / 100m);
            if (request.Amount > maxLoanAmount)
                throw new ArgumentException($"Loan amount cannot exceed {maxLoanAmount:C} (12.5% of total balance)");

            // Check if requested amount exceeds available balance
            var totalOutstandingLoans = await _context.Loans
                .Where(l => l.Status == "Approved" && l.RemainingAmount > 0)
                .SumAsync(l => l.RemainingAmount);

            var availableBalance = totalBalance - totalOutstandingLoans;
            if (request.Amount > availableBalance)
                throw new InvalidOperationException($"Insufficient funds. Available: {availableBalance:C}");

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

            loan.PaidAmount += request.Amount;
            loan.RemainingAmount -= request.Amount;

            if (loan.RemainingAmount <= 0)
            {
                loan.Status = "Paid";
                // Distribute interest profit to all active members (including admin)
                await DistributeInterestProfitAsync(loan);
            }

            _context.Loans.Update(loan);
            await _context.SaveChangesAsync();

            return await GetLoanByIdAsync(loanId);
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
                ActiveLoans = activeLoans
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

