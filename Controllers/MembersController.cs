using CommunityFinanceAPI.Models.DTOs;
using CommunityFinanceAPI.Models.Entities;
using CommunityFinanceAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CommunityFinanceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // REMOVE [Authorize] since we're using custom middleware
    public class MembersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IGoalService _goalService;
        private readonly IContributionService _contributionService;
        private readonly ILogger<MembersController> _logger;
        private readonly IAuthService _authService; // Add this

        public MembersController(
            IUserService userService,
            IGoalService goalService,
            IContributionService contributionService,
            ILogger<MembersController> logger,
            IAuthService authService) // Add this parameter
        {
            _userService = userService;
            _goalService = goalService;
            _contributionService = contributionService;
            _logger = logger;
            _authService = authService; // Initialize
        }

        // === NEW METHOD: Get current user profile using simple auth ===
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            // Get user from HttpContext (set by middleware)
            if (HttpContext.Items["AuthenticatedUser"] is not User user)
                return Unauthorized(new { Message = "User not authenticated. Please provide X-User-Email and X-User-Password headers." });

            // Return user profile (excluding password hash)
            return Ok(new
            {
                user.UserId,
                user.Email,
                user.FirstName,
                user.LastName,
                user.PhoneNumber,
                user.ProfilePictureUrl,
                user.Role,
                user.IsActive,
                user.CreatedAt,
                user.UpdatedAt
            });
        }

        // === NEW METHOD: Update profile using simple auth ===
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserRequest request)
        {
            try
            {
                var user = GetAuthenticatedUser();

                // Use your existing user service method
                var updatedUser = await _userService.UpdateUserAsync(user.UserId, request);

                _logger.LogInformation("User {UserId} updated their profile", user.UserId);
                return Ok(updatedUser);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("UpdateProfile: Unauthorized - {Message}", ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile: {Message}", ex.Message);
                _logger.LogError(ex, "Stack trace: {StackTrace}", ex.StackTrace);
                return StatusCode(500, new { message = $"An error occurred while updating profile: {ex.Message}" });
            }
        }

        // === HELPER METHOD: Get authenticated user ===
        private User GetAuthenticatedUser()
        {
            // Check if user is in HttpContext.Items
            if (HttpContext.Items["AuthenticatedUser"] is User user)
            {
                // #region agent log
                try {
                    System.IO.File.AppendAllText(@"d:\Dotnet\CommunityFinanceAPI\.cursor\debug.log", 
                        System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "D", location = "MembersController.GetAuthenticatedUser:96", message = "Authenticated user found", data = new { userId = user.UserId, email = user.Email, path = HttpContext.Request.Path }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n");
                } catch { /* Ignore file lock errors */ }
                // #endregion
                return user;
            }

            // Debug: Log what's in HttpContext.Items
            _logger.LogWarning("❌ GetAuthenticatedUser: No authenticated user found in HttpContext.Items");
            _logger.LogWarning("HttpContext.Items keys: {Keys}", string.Join(", ", HttpContext.Items.Keys));
            _logger.LogWarning("Request path: {Path}", HttpContext.Request.Path);
            _logger.LogWarning("Has X-User-Email header: {HasEmail}", HttpContext.Request.Headers.ContainsKey("X-User-Email"));
            _logger.LogWarning("Has X-User-Password header: {HasPassword}", HttpContext.Request.Headers.ContainsKey("X-User-Password"));
            
            // #region agent log
            try {
                System.IO.File.AppendAllText(@"d:\Dotnet\CommunityFinanceAPI\.cursor\debug.log", 
                    System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "D", location = "MembersController.GetAuthenticatedUser:106", message = "No authenticated user found", data = new { path = HttpContext.Request.Path, contextItemsKeys = string.Join(", ", HttpContext.Items.Keys), hasEmailHeader = HttpContext.Request.Headers.ContainsKey("X-User-Email"), hasPasswordHeader = HttpContext.Request.Headers.ContainsKey("X-User-Password") }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n");
            } catch { /* Ignore file lock errors */ }
            // #endregion

            throw new UnauthorizedAccessException("User not authenticated. Please provide X-User-Email and X-User-Password headers.");
        }

        // === HELPER METHOD: Get user ID ===
        private int GetAuthenticatedUserId()
        {
            var user = GetAuthenticatedUser();
            return user.UserId;
        }

        // === UPDATE ALL EXISTING METHODS: Replace User.Claims with simple auth ===

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var userId = GetAuthenticatedUserId(); // Use helper
                var stats = await _userService.GetUserStatsAsync(userId);
                return Ok(stats);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stats");
                return StatusCode(500, new { message = "An error occurred while retrieving stats" });
            }
        }

        [HttpGet("goals")]
        public async Task<IActionResult> GetGoals()
        {
            try
            {
                var userId = GetAuthenticatedUserId(); // Use helper
                var goals = await _userService.GetUserGoalsAsync(userId);
                return Ok(goals);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting goals");
                return StatusCode(500, new { message = "An error occurred while retrieving goals" });
            }
        }

        [HttpPost("goals/{goalId}/join")]
        public async Task<IActionResult> JoinGoal(int goalId, [FromBody] JoinGoalRequest request)
        {
            try
            {
                var userId = GetAuthenticatedUserId(); // Use helper
                var memberGoal = await _goalService.JoinGoalAsync(goalId, userId, request);

                _logger.LogInformation("User {UserId} joined goal {GoalId}", userId, goalId);
                return Ok(memberGoal);
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
                _logger.LogError(ex, "Error joining goal {GoalId}", goalId);
                return StatusCode(500, new { message = "An error occurred while joining goal" });
            }
        }

        [HttpDelete("goals/{goalId}/leave")]
        public async Task<IActionResult> LeaveGoal(int goalId)
        {
            try
            {
                var userId = GetAuthenticatedUserId(); // Use helper
                var result = await _goalService.LeaveGoalAsync(goalId, userId);

                if (result)
                {
                    _logger.LogInformation("User {UserId} left goal {GoalId}", userId, goalId);
                    return Ok(new { message = "Successfully left the goal" });
                }

                return BadRequest(new { message = "Failed to leave the goal" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error leaving goal {GoalId}", goalId);
                return StatusCode(500, new { message = "An error occurred while leaving goal" });
            }
        }

        [HttpGet("contributions")]
        public async Task<IActionResult> GetContributions()
        {
            try
            {
                var userId = GetAuthenticatedUserId(); // Use helper
                var contributions = await _contributionService.GetContributionsByUserAsync(userId);
                return Ok(contributions);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contributions");
                return StatusCode(500, new { message = "An error occurred while retrieving contributions" });
            }
        }

        [HttpPost("contributions")]
        public async Task<IActionResult> CreateContribution([FromBody] CreateContributionRequest request)
        {
            try
            {
                var userId = GetAuthenticatedUserId(); // Use helper
                var contribution = await _contributionService.CreateContributionAsync(request, userId);

                _logger.LogInformation("User {UserId} created contribution {ContributionId} for goal {GoalId}",
                    userId, contribution.ContributionId, request.GoalId);
                return Ok(contribution);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("CreateContribution: Unauthorized - {Message}", ex.Message);
                return Unauthorized(new { message = ex.Message });
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
                _logger.LogError(ex, "Error creating contribution: {Message}", ex.Message);
                _logger.LogError("Inner exception: {InnerException}", ex.InnerException?.Message ?? "None");
                _logger.LogError("Stack trace: {StackTrace}", ex.StackTrace);
                
                // Return more detailed error message for debugging
                // Unwrap InvalidOperationException to get the actual database error
                var errorMessage = ex.Message;
                if (ex is InvalidOperationException && ex.InnerException != null)
                {
                    errorMessage = ex.InnerException.Message;
                }
                else if (ex.InnerException != null)
                {
                    errorMessage = ex.InnerException.Message;
                }
                
                return StatusCode(500, new { message = $"An error occurred while creating contribution: {errorMessage}" });
            }
        }

        [HttpGet("contributions/stats")]
        public async Task<IActionResult> GetContributionStats()
        {
            try
            {
                var userId = GetAuthenticatedUserId(); // Use helper
                var stats = await _contributionService.GetUserContributionStatsAsync(userId);
                return Ok(stats);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contribution stats");
                return StatusCode(500, new { message = "An error occurred while retrieving contribution stats" });
            }
        }

        // === REMOVE OR KEEP DEBUG ENDPOINT (optional) ===
        [HttpGet("debug")]
        public IActionResult DebugAuth()
        {
            var user = GetAuthenticatedUser();

            var debugInfo = new
            {
                IsAuthenticated = user != null,
                UserId = user?.UserId,
                Email = user?.Email,
                Role = user?.Role,
                AuthHeadersProvided = Request.Headers.ContainsKey("X-User-Email") &&
                                     Request.Headers.ContainsKey("X-User-Password")
            };

            return Ok(debugInfo);
        }
        [HttpGet("all")]
        public async Task<IActionResult> GetAllMembers()
        {
            try
            {
                var currentUser = GetAuthenticatedUser();
                if (currentUser.Role != "Admin")
                    return Forbid();

                var members = await _userService.GetAllUsersAsync();

                return Ok(members); // ← This returns your perfect UserResponse list
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all members");
                return StatusCode(500, new { message = "Failed to load members" });
            }
        }
    }
}