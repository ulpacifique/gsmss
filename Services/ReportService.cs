using System.ComponentModel;
using System.Text;
using CommunityFinanceAPI.Data;
using CommunityFinanceAPI.Models.DTOs;
using CommunityFinanceAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace CommunityFinanceAPI.Services.Implementations
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;

        public ReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<FinancialReportResponse> GenerateFinancialReportAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            startDate ??= DateTime.Now.AddYears(-1);
            endDate ??= DateTime.Now;

            var report = new FinancialReportResponse
            {
                ReportDate = DateTime.Now
            };

            // Get total members
            report.TotalMembers = await _context.Users
                .Where(u => u.IsActive && u.Role == "Member")
                .CountAsync();

            // Get total goals
            report.TotalGoals = await _context.SavingsGoals.CountAsync();

            // Get total contributions (approved only)
            report.TotalContributions = await _context.Contributions
                .Where(c => c.Status == "Approved" && c.CreatedAt >= startDate && c.CreatedAt <= endDate)
                .SumAsync(c => c.Amount);

            // Get goal summaries - FIXED: Use Select projection instead of Include
            report.GoalSummaries = await _context.SavingsGoals
                .Select(g => new GoalSummary
                {
                    GoalName = g.GoalName,
                    TargetAmount = g.TargetAmount,
                    CurrentAmount = g.CurrentAmount,
                    ProgressPercentage = g.TargetAmount > 0 ? (g.CurrentAmount / g.TargetAmount) * 100 : 0,
                    MemberCount = g.MemberGoals.Count
                })
                .ToListAsync();

            // Get monthly contributions
            var monthlyContributions = await _context.Contributions
                .Where(c => c.Status == "Approved" && c.CreatedAt >= startDate && c.CreatedAt <= endDate)
                .GroupBy(c => new { c.CreatedAt.Year, c.CreatedAt.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Amount = g.Sum(c => c.Amount)
                })
                .OrderBy(g => g.Year)
                .ThenBy(g => g.Month)
                .ToListAsync();

            report.MonthlyContributions = monthlyContributions.Select(mc => new MonthlyContribution
            {
                Month = new DateTime(mc.Year, mc.Month, 1).ToString("MMMM"),
                Year = mc.Year,
                Amount = mc.Amount
            }).ToList();

            // Get top contributors
            var topContributors = await _context.Contributions
                .Where(c => c.Status == "Approved" && c.CreatedAt >= startDate && c.CreatedAt <= endDate)
                .GroupBy(c => c.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    TotalContributed = g.Sum(c => c.Amount),
                    ContributionCount = g.Count()
                })
                .OrderByDescending(g => g.TotalContributed)
                .Take(10)
                .ToListAsync();

            var userIds = topContributors.Select(tc => tc.UserId).ToList();
            var users = await _context.Users
                .Where(u => userIds.Contains(u.UserId))
                .ToDictionaryAsync(u => u.UserId, u => u.FullName);

            report.TopContributors = topContributors.Select(tc => new MemberContributionSummary
            {
                MemberName = users.ContainsKey(tc.UserId) ? users[tc.UserId] : "Unknown",
                TotalContributed = tc.TotalContributed,
                ContributionCount = tc.ContributionCount
            }).ToList();

            // Calculate current balance (simplified - total contributions minus any disbursements)
            report.CurrentBalance = report.TotalContributions;

            return report;
        }

        public async Task<AuditReportResponse> GenerateAuditReportAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            startDate ??= DateTime.Now.AddMonths(-1);
            endDate ??= DateTime.Now;

            var report = new AuditReportResponse();

            // Contribution audits
            var contributionChanges = await _context.Contributions
                .Where(c => c.CreatedAt >= startDate && c.CreatedAt <= endDate)
                .Include(c => c.User)
                .Include(c => c.ReviewedByUser)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            report.ContributionAudits = contributionChanges.Select(c => new AuditLog
            {
                Action = c.Status switch
                {
                    "Pending" => "Contribution Submitted",
                    "Approved" => "Contribution Approved",
                    "Rejected" => "Contribution Rejected",
                    _ => "Contribution Updated"
                },
                EntityType = "Contribution",
                UserName = c.User?.FullName ?? "Unknown",
                Timestamp = c.CreatedAt,
                Details = $"Amount: {c.Amount:C}, Reference: {c.PaymentReference}, Status: {c.Status}"
            }).ToList();

            // Goal audits (simplified - tracks goal creation and status changes)
            var goalChanges = await _context.SavingsGoals
                .Where(g => g.CreatedAt >= startDate && g.CreatedAt <= endDate)
                .Include(g => g.CreatedByUser)
                .OrderByDescending(g => g.CreatedAt)
                .ToListAsync();

            report.GoalAudits = goalChanges.Select(g => new AuditLog
            {
                Action = "Goal Created",
                EntityType = "Goal",
                UserName = g.CreatedByUser?.FullName ?? "Unknown",
                Timestamp = g.CreatedAt,
                Details = $"Goal: {g.GoalName}, Target: {g.TargetAmount:C}, Status: {g.Status}"
            }).ToList();

            // User audits (tracks user registration)
            var userChanges = await _context.Users
                .Where(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            report.UserAudits = userChanges.Select(u => new AuditLog
            {
                Action = "User Registered",
                EntityType = "User",
                UserName = u.FullName,
                Timestamp = u.CreatedAt,
                Details = $"Email: {u.Email}, Role: {u.Role}, Status: {(u.IsActive ? "Active" : "Inactive")}"
            }).ToList();

            return report;
        }

        public async Task<byte[]> ExportContributionsToExcelAsync(int? userId = null, int? goalId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Contributions
                .Include(c => c.User)
                .Include(c => c.Goal)
                .Include(c => c.ReviewedByUser)
                .AsQueryable();

            if (userId.HasValue)
                query = query.Where(c => c.UserId == userId.Value);

            if (goalId.HasValue)
                query = query.Where(c => c.GoalId == goalId.Value);

            if (startDate.HasValue)
                query = query.Where(c => c.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(c => c.CreatedAt <= endDate.Value);

            var contributions = await query
                .OrderByDescending(c => c.SubmittedAt)
                .ToListAsync();

            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Contributions");

            // Add headers
            worksheet.Cells[1, 1].Value = "ID";
            worksheet.Cells[1, 2].Value = "Member";
            worksheet.Cells[1, 3].Value = "Goal";
            worksheet.Cells[1, 4].Value = "Amount";
            worksheet.Cells[1, 5].Value = "Payment Reference";
            worksheet.Cells[1, 6].Value = "Status";
            worksheet.Cells[1, 7].Value = "Submitted Date";
            worksheet.Cells[1, 8].Value = "Reviewed By";
            worksheet.Cells[1, 9].Value = "Reviewed Date";
            worksheet.Cells[1, 10].Value = "Rejection Reason";

            // Add data
            for (int i = 0; i < contributions.Count; i++)
            {
                var row = i + 2;
                var c = contributions[i];

                worksheet.Cells[row, 1].Value = c.ContributionId;
                worksheet.Cells[row, 2].Value = c.User?.FullName;
                worksheet.Cells[row, 3].Value = c.Goal?.GoalName;
                worksheet.Cells[row, 4].Value = c.Amount;
                worksheet.Cells[row, 4].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[row, 5].Value = c.PaymentReference;
                worksheet.Cells[row, 6].Value = c.Status;
                worksheet.Cells[row, 7].Value = c.SubmittedAt.ToString("yyyy-MM-dd HH:mm");
                worksheet.Cells[row, 8].Value = c.ReviewedByUser?.FullName;
                worksheet.Cells[row, 9].Value = c.ReviewedAt?.ToString("yyyy-MM-dd HH:mm");
                worksheet.Cells[row, 10].Value = c.RejectionReason;
            }

            // Auto-fit columns
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            return await package.GetAsByteArrayAsync();
        }

        public async Task<byte[]> ExportMembersToExcelAsync()
        {
            var members = await _context.Users
                .Where(u => u.Role == "Member" && u.IsActive)
                .Include(u => u.MemberGoals)
                .Include(u => u.Contributions)
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToListAsync();

            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Members");

            // Add headers
            worksheet.Cells[1, 1].Value = "ID";
            worksheet.Cells[1, 2].Value = "Name";
            worksheet.Cells[1, 3].Value = "Email";
            worksheet.Cells[1, 4].Value = "Phone";
            worksheet.Cells[1, 5].Value = "Joined Date";
            worksheet.Cells[1, 6].Value = "Active Goals";
            worksheet.Cells[1, 7].Value = "Total Contributed";
            worksheet.Cells[1, 8].Value = "Last Contribution";

            // Add data
            for (int i = 0; i < members.Count; i++)
            {
                var row = i + 2;
                var member = members[i];

                var totalContributed = member.Contributions
                    .Where(c => c.Status == "Approved")
                    .Sum(c => c.Amount);

                var lastContribution = member.Contributions
                    .OrderByDescending(c => c.CreatedAt)
                    .FirstOrDefault();

                worksheet.Cells[row, 1].Value = member.UserId;
                worksheet.Cells[row, 2].Value = member.FullName;
                worksheet.Cells[row, 3].Value = member.Email;
                worksheet.Cells[row, 4].Value = member.PhoneNumber;
                worksheet.Cells[row, 5].Value = member.CreatedAt.ToString("yyyy-MM-dd");
                worksheet.Cells[row, 6].Value = member.MemberGoals.Count(mg => mg.Goal.Status == "Active");
                worksheet.Cells[row, 7].Value = totalContributed;
                worksheet.Cells[row, 7].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[row, 8].Value = lastContribution?.CreatedAt.ToString("yyyy-MM-dd");
            }

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            return await package.GetAsByteArrayAsync();
        }

        public async Task<byte[]> ExportGoalsToExcelAsync()
        {
            var goals = await _context.SavingsGoals
                .Include(g => g.CreatedByUser)
                .Include(g => g.MemberGoals)
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
                    CreatedByName = g.CreatedByUser != null ? g.CreatedByUser.FullName : "Unknown",
                    g.CreatedAt,
                    MemberCount = g.MemberGoals.Count,
                    Progress = g.TargetAmount > 0 ? (g.CurrentAmount / g.TargetAmount) * 100 : 0
                })
                .OrderByDescending(g => g.CreatedAt)
                .ToListAsync();

            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Goals");

            // Add headers
            worksheet.Cells[1, 1].Value = "ID";
            worksheet.Cells[1, 2].Value = "Goal Name";
            worksheet.Cells[1, 3].Value = "Description";
            worksheet.Cells[1, 4].Value = "Target Amount";
            worksheet.Cells[1, 5].Value = "Current Amount";
            worksheet.Cells[1, 6].Value = "Progress %";
            worksheet.Cells[1, 7].Value = "Start Date";
            worksheet.Cells[1, 8].Value = "End Date";
            worksheet.Cells[1, 9].Value = "Status";
            worksheet.Cells[1, 10].Value = "Created By";
            worksheet.Cells[1, 11].Value = "Members";
            worksheet.Cells[1, 12].Value = "Created Date";

            // Add data
            for (int i = 0; i < goals.Count; i++)
            {
                var row = i + 2;
                var goal = goals[i];

                worksheet.Cells[row, 1].Value = goal.GoalId;
                worksheet.Cells[row, 2].Value = goal.GoalName;
                worksheet.Cells[row, 3].Value = goal.Description;
                worksheet.Cells[row, 4].Value = goal.TargetAmount;
                worksheet.Cells[row, 4].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[row, 5].Value = goal.CurrentAmount;
                worksheet.Cells[row, 5].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[row, 6].Value = goal.Progress;
                worksheet.Cells[row, 6].Style.Numberformat.Format = "0.00%";
                worksheet.Cells[row, 7].Value = goal.StartDate.ToString("yyyy-MM-dd");
                worksheet.Cells[row, 8].Value = goal.EndDate.ToString("yyyy-MM-dd");
                worksheet.Cells[row, 9].Value = goal.Status;
                worksheet.Cells[row, 10].Value = goal.CreatedByName;
                worksheet.Cells[row, 11].Value = goal.MemberCount;
                worksheet.Cells[row, 12].Value = goal.CreatedAt.ToString("yyyy-MM-dd");
            }

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            return await package.GetAsByteArrayAsync();
        }
    }
}