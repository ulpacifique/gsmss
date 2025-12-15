using CommunityFinanceAPI.Models.DTOs;
using CommunityFinanceAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommunityFinanceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
   // [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IGoalService _goalService;
        private readonly IContributionService _contributionService;
        private readonly IReportService _reportService;
        private readonly ILoanService _loanService;
        private readonly IContributionLimitService _contributionLimitService;
        private readonly IPermissionService _permissionService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            IUserService userService,
            IGoalService goalService,
            IContributionService contributionService,
            IReportService reportService,
            ILoanService loanService,
            IContributionLimitService contributionLimitService,
            IPermissionService permissionService,
            ILogger<AdminController> logger)
        {
            _userService = userService;
            _goalService = goalService;
            _contributionService = contributionService;
            _reportService = reportService;
            _loanService = loanService;
            _contributionLimitService = contributionLimitService;
            _permissionService = permissionService;
            _logger = logger;
        }

        // User Management
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users");
                return StatusCode(500, new { message = "An error occurred while retrieving users" });
            }
        }

        [HttpGet("users/members")]
        public async Task<IActionResult> GetMembers()
        {
            try
            {
                // Check authentication
                if (HttpContext.Items["AuthenticatedUser"] is not Models.Entities.User user)
                {
                    return Unauthorized(new { message = "User not authenticated. Please provide X-User-Email and X-User-Password headers." });
                }

                // Check if user is admin
                if (user.Role != "Admin")
                {
                    return Forbid();
                }

                var members = await _userService.GetUsersByRoleAsync("Member");
                return Ok(members);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting members");
                return StatusCode(500, new { message = "An error occurred while retrieving members" });
            }
        }

        [HttpGet("users/admins")]
        public async Task<IActionResult> GetAdmins()
        {
            try
            {
                var admins = await _userService.GetUsersByRoleAsync("Admin");
                return Ok(admins);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting admins");
                return StatusCode(500, new { message = "An error occurred while retrieving admins" });
            }
        }

        [HttpGet("users/{userId}")]
        public async Task<IActionResult> GetUserById(int userId)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(userId);
                return Ok(user);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user {UserId}", userId);
                return StatusCode(500, new { message = "An error occurred while retrieving user" });
            }
        }

        [HttpPut("users/{userId}/deactivate")]
        public async Task<IActionResult> DeactivateUser(int userId)
        {
            try
            {
                var result = await _userService.DeactivateUserAsync(userId);

                if (result)
                {
                    _logger.LogInformation("Admin deactivated user {UserId}", userId);
                    return Ok(new { message = "User deactivated successfully" });
                }

                return BadRequest(new { message = "Failed to deactivate user" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating user {UserId}", userId);
                return StatusCode(500, new { message = "An error occurred while deactivating user" });
            }
        }

        [HttpDelete("users/{userId}")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            try
            {
                var currentUser = GetAuthenticatedUserId();
                if (userId == currentUser)
                {
                    return BadRequest(new { message = "You cannot delete your own account" });
                }

                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                // Check if user has contributions or loans - if so, deactivate instead of delete
                var hasContributions = await _contributionService.GetContributionsByUserAsync(userId);
                if (hasContributions.Any())
                {
                    // Deactivate instead of delete
                    var result = await _userService.DeactivateUserAsync(userId);
                    if (result)
                    {
                        _logger.LogInformation("Admin deactivated user {UserId} (has contributions)", userId);
                        return Ok(new { message = "User deactivated successfully (user has contributions, cannot be deleted)" });
                    }
                }

                // For now, just deactivate. Full delete would require cascade delete handling
                var deactivateResult = await _userService.DeactivateUserAsync(userId);
                if (deactivateResult)
                {
                    _logger.LogInformation("Admin deactivated user {UserId}", userId);
                    return Ok(new { message = "User deactivated successfully" });
                }

                return BadRequest(new { message = "Failed to deactivate user" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", userId);
                return StatusCode(500, new { message = "An error occurred while deleting user" });
            }
        }

        [HttpPut("users/{userId}/activate")]
        public async Task<IActionResult> ActivateUser(int userId)
        {
            try
            {
                var result = await _userService.ActivateUserAsync(userId);

                if (result)
                {
                    _logger.LogInformation("Admin activated user {UserId}", userId);
                    return Ok(new { message = "User activated successfully" });
                }

                return BadRequest(new { message = "Failed to activate user" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating user {UserId}", userId);
                return StatusCode(500, new { message = "An error occurred while activating user" });
            }
        }

        [HttpPut("users/{userId}")]
        public async Task<IActionResult> UpdateUser(int userId, [FromBody] UpdateUserRequest request)
        {
            try
            {
                var updatedUser = await _userService.UpdateUserAsync(userId, request);
                _logger.LogInformation("Admin updated user {UserId}", userId);
                return Ok(updatedUser);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", userId);
                return StatusCode(500, new { message = "An error occurred while updating user" });
            }
        }

        // Contribution Management
        [HttpGet("contributions")]
        public async Task<IActionResult> GetAllContributions()
        {
            try
            {
                var contributions = await _contributionService.GetAllContributionsAsync();
                return Ok(contributions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all contributions");
                return StatusCode(500, new { message = "An error occurred while retrieving contributions" });
            }
        }

        [HttpGet("contributions/pending")]
        public async Task<IActionResult> GetPendingContributions()
        {
            try
            {
                var contributions = await _contributionService.GetPendingContributionsAsync();
                return Ok(contributions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending contributions");
                return StatusCode(500, new { message = "An error occurred while retrieving pending contributions" });
            }
        }

        [HttpPut("contributions/{contributionId}/status")]
        public async Task<IActionResult> UpdateContributionStatus(int contributionId, [FromBody] UpdateContributionStatusRequest request)
        {
            try
            {
                var reviewedBy = GetAuthenticatedUserId();
                var contribution = await _contributionService.UpdateContributionStatusAsync(contributionId, request, reviewedBy);

                _logger.LogInformation("Admin {AdminId} updated contribution {ContributionId} status to {Status}",
                    reviewedBy, contributionId, request.Status);

                return Ok(contribution);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating contribution {ContributionId} status", contributionId);
                return StatusCode(500, new { message = "An error occurred while updating contribution status" });
            }
        }

        [HttpGet("contributions/stats")]
        public async Task<IActionResult> GetContributionStats()
        {
            try
            {
                var stats = await _contributionService.GetContributionStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contribution stats");
                return StatusCode(500, new { message = "An error occurred while retrieving contribution stats" });
            }
        }

        // Loan Management
        [HttpGet("loans")]
        public async Task<IActionResult> GetAllLoans()
        {
            try
            {
                var loans = await _loanService.GetAllLoansAsync();
                return Ok(loans);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all loans");
                return StatusCode(500, new { message = "An error occurred while retrieving loans" });
            }
        }

        [HttpGet("loans/pending")]
        public async Task<IActionResult> GetPendingLoans()
        {
            try
            {
                var loans = await _loanService.GetPendingLoansAsync();
                return Ok(loans);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending loans");
                return StatusCode(500, new { message = "An error occurred while retrieving pending loans" });
            }
        }

        [HttpPut("loans/{loanId}/approve")]
        public async Task<IActionResult> ApproveLoan(int loanId, [FromBody] ApproveLoanRequest? request = null)
        {
            try
            {
                var approvedBy = GetAuthenticatedUserId();
                var loan = await _loanService.ApproveLoanAsync(loanId, approvedBy, request);
                _logger.LogInformation("Admin {AdminId} approved loan {LoanId}", approvedBy, loanId);
                return Ok(loan);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving loan {LoanId}", loanId);
                return StatusCode(500, new { message = "An error occurred while approving loan" });
            }
        }

        [HttpPut("loans/{loanId}/reject")]
        public async Task<IActionResult> RejectLoan(int loanId, [FromBody] RejectLoanRequest request)
        {
            try
            {
                var rejectedBy = GetAuthenticatedUserId();
                var loan = await _loanService.RejectLoanAsync(loanId, rejectedBy, request);
                _logger.LogInformation("Admin {AdminId} rejected loan {LoanId}", rejectedBy, loanId);
                return Ok(loan);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting loan {LoanId}", loanId);
                return StatusCode(500, new { message = "An error occurred while rejecting loan" });
            }
        }

        // Reports
        [HttpGet("reports/financial")]
        public async Task<IActionResult> GetFinancialReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
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

        [HttpGet("reports/audit")]
        public async Task<IActionResult> GetAuditReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
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

        // Export endpoints
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

        // Dashboard Stats
        [HttpGet("dashboard/stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var goalStats = await _goalService.GetGoalStatsAsync();
                var contributionStats = await _contributionService.GetContributionStatsAsync();
                var totalMembers = await _userService.GetUsersByRoleAsync("Member");
                var pendingContributions = await _contributionService.GetPendingContributionsAsync();

                var dashboardStats = new
                {
                    GoalStats = goalStats,
                    ContributionStats = contributionStats,
                    TotalMembers = totalMembers.Count(),
                    PendingContributionsCount = pendingContributions.Count()
                };

                return Ok(dashboardStats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard stats");
                return StatusCode(500, new { message = "An error occurred while retrieving dashboard stats" });
            }
        }

        // Contribution Limits Management
        [HttpPost("contribution-limits")]
        public async Task<IActionResult> CreateContributionLimit([FromBody] CreateContributionLimitRequest request)
        {
            try
            {
                var limit = await _contributionLimitService.CreateContributionLimitAsync(request);
                return Ok(limit);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating contribution limit");
                return StatusCode(500, new { message = "An error occurred while creating contribution limit" });
            }
        }

        [HttpPut("contribution-limits/{limitId}")]
        public async Task<IActionResult> UpdateContributionLimit(int limitId, [FromBody] UpdateContributionLimitRequest request)
        {
            try
            {
                var limit = await _contributionLimitService.UpdateContributionLimitAsync(limitId, request);
                return Ok(limit);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating contribution limit");
                return StatusCode(500, new { message = "An error occurred while updating contribution limit" });
            }
        }

        [HttpGet("contribution-limits")]
        public async Task<IActionResult> GetAllContributionLimits()
        {
            try
            {
                var limits = await _contributionLimitService.GetAllContributionLimitsAsync();
                return Ok(limits);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contribution limits");
                return StatusCode(500, new { message = "An error occurred while retrieving contribution limits" });
            }
        }

        [HttpGet("contribution-limits/goal/{goalId}")]
        public async Task<IActionResult> GetContributionLimitByGoal(int goalId)
        {
            try
            {
                var limit = await _contributionLimitService.GetContributionLimitByGoalIdAsync(goalId);
                if (limit == null)
                    return NotFound(new { message = "No contribution limit found for this goal" });
                return Ok(limit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contribution limit for goal");
                return StatusCode(500, new { message = "An error occurred while retrieving contribution limit" });
            }
        }

        [HttpDelete("contribution-limits/{limitId}")]
        public async Task<IActionResult> DeleteContributionLimit(int limitId)
        {
            try
            {
                await _contributionLimitService.DeleteContributionLimitAsync(limitId);
                return Ok(new { message = "Contribution limit deleted successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting contribution limit");
                return StatusCode(500, new { message = "An error occurred while deleting contribution limit" });
            }
        }

        private int GetAuthenticatedUserId()
        {
            // Try to get from UserId first
            if (HttpContext.Items["UserId"] is int userId)
                return userId;
            
            // Fallback to getting from AuthenticatedUser
            if (HttpContext.Items["AuthenticatedUser"] is Models.Entities.User user)
                return user.UserId;
            
            throw new UnauthorizedAccessException("User not authenticated");
        }
    }
}