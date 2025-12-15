using CommunityFinanceAPI.Data;
using CommunityFinanceAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CommunityFinanceAPI.Services.BackgroundServices
{
    public class OverdueLoanCheckerService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OverdueLoanCheckerService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(24); // Check once per day

        public OverdueLoanCheckerService(
            IServiceProvider serviceProvider,
            ILogger<OverdueLoanCheckerService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckOverdueLoansAsync();
                    await Task.Delay(_checkInterval, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error checking overdue loans");
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken); // Retry after 1 hour on error
                }
            }
        }

        private async Task CheckOverdueLoansAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            var now = DateTime.UtcNow;
            var overdueLoans = await context.Loans
                .AsNoTracking()
                .Where(l => l.Status == "Approved" 
                    && l.RemainingAmount > 0 
                    && l.DueDate < now)
                .Join(context.Users,
                    l => l.UserId,
                    u => u.UserId,
                    (l, u) => new
                    {
                        l.LoanId,
                        l.UserId,
                        l.PrincipalAmount,
                        l.RemainingAmount,
                        l.DueDate,
                        UserFirstName = u.FirstName,
                        UserLastName = u.LastName
                    })
                .ToListAsync();

            foreach (var loan in overdueLoans)
            {
                var daysOverdue = (int)(now - loan.DueDate).TotalDays;

                // Notify the member
                await notificationService.CreateNotificationAsync(new Models.DTOs.CreateNotificationRequest
                {
                    UserId = loan.UserId,
                    Title = "Loan Overdue",
                    Message = $"Your loan of {loan.PrincipalAmount:C} is overdue by {daysOverdue} day(s). Please make a payment as soon as possible. Due date was: {loan.DueDate:MM/dd/yyyy}",
                    Type = "warning",
                    RelatedEntityType = "Loan",
                    RelatedEntityId = loan.LoanId
                });

                // Notify admins
                var admins = await context.Users
                    .Where(u => u.Role == "Admin" && u.IsActive)
                    .ToListAsync();

                foreach (var admin in admins)
                {
                    await notificationService.CreateNotificationAsync(new Models.DTOs.CreateNotificationRequest
                    {
                        UserId = admin.UserId,
                        Title = "Member Loan Overdue",
                        Message = $"Member {loan.UserFirstName} {loan.UserLastName} has an overdue loan of {loan.PrincipalAmount:C}. Overdue by {daysOverdue} day(s). Remaining balance: {loan.RemainingAmount:C}",
                        Type = "warning",
                        RelatedEntityType = "Loan",
                        RelatedEntityId = loan.LoanId
                    });
                }

                _logger.LogInformation($"Notified about overdue loan {loan.LoanId} for user {loan.UserId}, {daysOverdue} days overdue");
            }

            if (overdueLoans.Any())
            {
                _logger.LogInformation($"Checked {overdueLoans.Count} overdue loans");
            }
        }
    }
}

