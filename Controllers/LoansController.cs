using CommunityFinanceAPI.Models.DTOs;
using CommunityFinanceAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CommunityFinanceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoansController : ControllerBase
    {
        private readonly ILoanService _loanService;
        private readonly ILogger<LoansController> _logger;

        public LoansController(ILoanService loanService, ILogger<LoansController> logger)
        {
            _loanService = loanService;
            _logger = logger;
        }

        [HttpPost("request")]
        public async Task<IActionResult> RequestLoan([FromBody] CreateLoanRequest request)
        {
            try
            {
                // Check if user is authenticated
                if (HttpContext.Items["AuthenticatedUser"] is not Models.Entities.User user)
                {
                    _logger.LogWarning("RequestLoan: User not authenticated");
                    // #region agent log
                    try {
                        System.IO.File.AppendAllText(@"d:\Dotnet\CommunityFinanceAPI\.cursor\debug.log", 
                            System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "D", location = "LoansController.RequestLoan:26", message = "User not authenticated in RequestLoan", data = new { path = HttpContext.Request.Path, contextItemsKeys = string.Join(", ", HttpContext.Items.Keys), hasEmailHeader = HttpContext.Request.Headers.ContainsKey("X-User-Email"), hasPasswordHeader = HttpContext.Request.Headers.ContainsKey("X-User-Password") }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n");
                    } catch { /* Ignore file lock errors */ }
                    // #endregion
                    return Unauthorized(new { message = "User not authenticated. Please provide X-User-Email and X-User-Password headers." });
                }
                
                // #region agent log
                try {
                    System.IO.File.AppendAllText(@"d:\Dotnet\CommunityFinanceAPI\.cursor\debug.log", 
                        System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "D", location = "LoansController.RequestLoan:32", message = "User authenticated in RequestLoan", data = new { userId = user.UserId, email = user.Email }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n");
                } catch { /* Ignore file lock errors */ }
                // #endregion

                var loan = await _loanService.RequestLoanAsync(user.UserId, request);
                _logger.LogInformation("User {UserId} requested loan {LoanId}", user.UserId, loan.LoanId);
                return Ok(loan);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("RequestLoan: UnauthorizedAccessException - {Message}", ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("RequestLoan: InvalidOperationException - {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("RequestLoan: ArgumentException - {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting loan: {Message}", ex.Message);
                return StatusCode(500, new { message = $"An error occurred while requesting loan: {ex.Message}" });
            }
        }

        [HttpPost("{loanId}/approve")]
        public async Task<IActionResult> ApproveLoan(int loanId, [FromBody] ApproveLoanRequest? request = null)
        {
            try
            {
                var approvedBy = GetAuthenticatedUserId();
                var userRole = GetAuthenticatedUserRole();
                
                // Only Secretary or Admin can approve loans
                if (userRole != "Secretary" && userRole != "Admin")
                    return Forbid();

                var loan = await _loanService.ApproveLoanAsync(loanId, approvedBy, request);
                _logger.LogInformation("User {UserId} approved loan {LoanId}", approvedBy, loanId);
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

        [HttpPost("{loanId}/reject")]
        public async Task<IActionResult> RejectLoan(int loanId, [FromBody] RejectLoanRequest request)
        {
            try
            {
                var rejectedBy = GetAuthenticatedUserId();
                var userRole = GetAuthenticatedUserRole();
                
                // Only Secretary or Admin can reject loans
                if (userRole != "Secretary" && userRole != "Admin")
                    return Forbid();

                var loan = await _loanService.RejectLoanAsync(loanId, rejectedBy, request);
                _logger.LogInformation("User {UserId} rejected loan {LoanId}", rejectedBy, loanId);
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

        [HttpPost("{loanId}/pay")]
        public async Task<IActionResult> PayLoan(int loanId, [FromBody] PayLoanRequest request)
        {
            try
            {
                var userId = GetAuthenticatedUserId();
                var loan = await _loanService.PayLoanAsync(loanId, userId, request);
                _logger.LogInformation("User {UserId} paid {Amount} towards loan {LoanId}", userId, request.Amount, loanId);
                return Ok(loan);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error paying loan {LoanId}", loanId);
                return StatusCode(500, new { message = "An error occurred while paying loan" });
            }
        }

        [HttpGet("my-loans")]
        public async Task<IActionResult> GetMyLoans()
        {
            try
            {
                var userId = GetAuthenticatedUserId();
                var loans = await _loanService.GetUserLoansAsync(userId);
                return Ok(loans);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user loans");
                return StatusCode(500, new { message = "An error occurred while retrieving loans" });
            }
        }

        [HttpGet("my-account")]
        public async Task<IActionResult> GetMyAccount()
        {
            try
            {
                var userId = GetAuthenticatedUserId();
                var account = await _loanService.GetMemberAccountAsync(userId);
                return Ok(account);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting member account");
                return StatusCode(500, new { message = "An error occurred while retrieving account" });
            }
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingLoans()
        {
            try
            {
                var userRole = GetAuthenticatedUserRole();
                if (userRole != "Secretary" && userRole != "Admin")
                    return Forbid();

                var loans = await _loanService.GetPendingLoansAsync();
                return Ok(loans);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending loans");
                return StatusCode(500, new { message = "An error occurred while retrieving pending loans" });
            }
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetLoanStats()
        {
            try
            {
                var userRole = GetAuthenticatedUserRole();
                if (userRole != "Secretary" && userRole != "Admin")
                    return Forbid();

                var stats = await _loanService.GetLoanStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting loan stats");
                return StatusCode(500, new { message = "An error occurred while retrieving loan stats" });
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

        private string GetAuthenticatedUserRole()
        {
            // Try to get from UserRole first
            if (HttpContext.Items["UserRole"] is string role)
                return role;
            
            // Fallback to getting from AuthenticatedUser
            if (HttpContext.Items["AuthenticatedUser"] is Models.Entities.User user)
                return user.Role;
            
            throw new UnauthorizedAccessException("User role not found");
        }
    }
}

