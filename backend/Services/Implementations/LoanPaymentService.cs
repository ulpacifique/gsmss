using CommunityFinanceAPI.Data;
using CommunityFinanceAPI.Models.DTOs;
using CommunityFinanceAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CommunityFinanceAPI.Services.Implementations
{
    public class LoanPaymentService : ILoanPaymentService
    {
        private readonly ApplicationDbContext _context;

        public LoanPaymentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<LoanPaymentResponse>> GetLoanPaymentsAsync(int loanId)
        {
            return await _context.LoanPayments
                .AsNoTracking()
                .Where(lp => lp.LoanId == loanId)
                .Join(_context.Users,
                    lp => lp.UserId,
                    u => u.UserId,
                    (lp, u) => new LoanPaymentResponse
                    {
                        PaymentId = lp.PaymentId,
                        LoanId = lp.LoanId,
                        UserId = lp.UserId,
                        UserName = $"{u.FirstName} {u.LastName}",
                        Amount = lp.Amount,
                        PaymentReference = lp.PaymentReference,
                        PaymentDate = lp.PaymentDate,
                        Notes = lp.Notes,
                        CreatedAt = lp.CreatedAt
                    })
                .OrderByDescending(lp => lp.PaymentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<LoanPaymentResponse>> GetUserLoanPaymentsAsync(int userId)
        {
            return await _context.LoanPayments
                .AsNoTracking()
                .Where(lp => lp.UserId == userId)
                .Join(_context.Users,
                    lp => lp.UserId,
                    u => u.UserId,
                    (lp, u) => new LoanPaymentResponse
                    {
                        PaymentId = lp.PaymentId,
                        LoanId = lp.LoanId,
                        UserId = lp.UserId,
                        UserName = $"{u.FirstName} {u.LastName}",
                        Amount = lp.Amount,
                        PaymentReference = lp.PaymentReference,
                        PaymentDate = lp.PaymentDate,
                        Notes = lp.Notes,
                        CreatedAt = lp.CreatedAt
                    })
                .OrderByDescending(lp => lp.PaymentDate)
                .ToListAsync();
        }

        public async Task<LoanPaymentResponse> GetLoanPaymentByIdAsync(int paymentId)
        {
            var payment = await _context.LoanPayments
                .AsNoTracking()
                .Where(lp => lp.PaymentId == paymentId)
                .Join(_context.Users,
                    lp => lp.UserId,
                    u => u.UserId,
                    (lp, u) => new LoanPaymentResponse
                    {
                        PaymentId = lp.PaymentId,
                        LoanId = lp.LoanId,
                        UserId = lp.UserId,
                        UserName = $"{u.FirstName} {u.LastName}",
                        Amount = lp.Amount,
                        PaymentReference = lp.PaymentReference,
                        PaymentDate = lp.PaymentDate,
                        Notes = lp.Notes,
                        CreatedAt = lp.CreatedAt
                    })
                .FirstOrDefaultAsync();

            if (payment == null)
                throw new KeyNotFoundException("Loan payment not found");

            return payment;
        }
    }
}

