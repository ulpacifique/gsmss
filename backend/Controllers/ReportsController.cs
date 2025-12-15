using CommunityFinanceAPI.Models.DTOs;
using CommunityFinanceAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommunityFinanceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "Admin")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(IReportService reportService, ILogger<ReportsController> logger)
        {
            _reportService = reportService;
            _logger = logger;
        }

        [HttpGet("financial")]
        public async Task<IActionResult> GetFinancialReport(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            try
            {
                var report = await _reportService.GenerateFinancialReportAsync(startDate, endDate);
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating financial report");
                return StatusCode(500, new { message = "An error occurred while generating financial report" });
            }
        }

        [HttpGet("audit")]
        public async Task<IActionResult> GetAuditReport(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            try
            {
                var report = await _reportService.GenerateAuditReportAsync(startDate, endDate);
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating audit report");
                return StatusCode(500, new { message = "An error occurred while generating audit report" });
            }
        }

        [HttpGet("export/contributions")]
        public async Task<IActionResult> ExportContributions(
            [FromQuery] int? userId,
            [FromQuery] int? goalId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            try
            {
                var excelBytes = await _reportService.ExportContributionsToExcelAsync(userId, goalId, startDate, endDate);
                return File(excelBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"contributions_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting contributions");
                return StatusCode(500, new { message = "An error occurred while exporting contributions" });
            }
        }

        [HttpGet("export/members")]
        public async Task<IActionResult> ExportMembers()
        {
            try
            {
                var excelBytes = await _reportService.ExportMembersToExcelAsync();
                return File(excelBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"members_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting members");
                return StatusCode(500, new { message = "An error occurred while exporting members" });
            }
        }

        [HttpGet("export/goals")]
        public async Task<IActionResult> ExportGoals()
        {
            try
            {
                var excelBytes = await _reportService.ExportGoalsToExcelAsync();
                return File(excelBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"goals_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting goals");
                return StatusCode(500, new { message = "An error occurred while exporting goals" });
            }
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardSummary()
        {
            try
            {
                // Get current month and previous month
                var currentDate = DateTime.Now;
                var currentMonthStart = new DateTime(currentDate.Year, currentDate.Month, 1);
                var lastMonthStart = currentMonthStart.AddMonths(-1);
                var lastMonthEnd = currentMonthStart.AddDays(-1);

                // Generate reports for comparison
                var currentMonthReport = await _reportService.GenerateFinancialReportAsync(currentMonthStart, currentDate);
                var lastMonthReport = await _reportService.GenerateFinancialReportAsync(lastMonthStart, lastMonthEnd);

                var dashboardSummary = new
                {
                    CurrentMonth = new
                    {
                        TotalContributions = currentMonthReport.TotalContributions,
                        TotalMembers = currentMonthReport.TotalMembers,
                        NewContributions = currentMonthReport.MonthlyContributions.LastOrDefault()?.Amount ?? 0
                    },
                    LastMonth = new
                    {
                        TotalContributions = lastMonthReport.TotalContributions,
                        TotalMembers = lastMonthReport.TotalMembers
                    },
                    ActiveGoals = currentMonthReport.GoalSummaries.Count(g => g.ProgressPercentage < 100),
                    TopContributors = currentMonthReport.TopContributors.Take(5).ToList()
                };

                return Ok(dashboardSummary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating dashboard summary");
                return StatusCode(500, new { message = "An error occurred while generating dashboard summary" });
            }
        }

        [HttpGet("monthly-trends")]
        public async Task<IActionResult> GetMonthlyTrends([FromQuery] int months = 6)
        {
            try
            {
                var endDate = DateTime.Now;
                var startDate = endDate.AddMonths(-months);

                var report = await _reportService.GenerateFinancialReportAsync(startDate, endDate);

                var trends = report.MonthlyContributions
                    .OrderBy(mc => mc.Year)
                    .ThenBy(mc =>
                    {
                        var months = new[] { "January", "February", "March", "April", "May", "June",
                                           "July", "August", "September", "October", "November", "December" };
                        return Array.IndexOf(months, mc.Month);
                    })
                    .Select(mc => new
                    {
                        Period = $"{mc.Month} {mc.Year}",
                        Amount = mc.Amount
                    })
                    .ToList();

                return Ok(trends);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting monthly trends");
                return StatusCode(500, new { message = "An error occurred while retrieving monthly trends" });
            }
        }

    }
}