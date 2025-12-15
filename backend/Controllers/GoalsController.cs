using CommunityFinanceAPI.Models.DTOs;
using CommunityFinanceAPI.Models.Entities;
using CommunityFinanceAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommunityFinanceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
   // [Authorize]
    public class GoalsController : ControllerBase
    {
        private readonly IGoalService _goalService;
        private readonly IContributionService _contributionService;
        private readonly ILogger<GoalsController> _logger;

        public GoalsController(IGoalService goalService, IContributionService contributionService, ILogger<GoalsController> logger)
        {
            _goalService = goalService;
            _contributionService = contributionService;
            _logger = logger;
        }

        // Public endpoints (accessible to all authenticated users)
        [HttpGet]
        public async Task<IActionResult> GetAllGoals()
        {
            try
            {
                var goals = await _goalService.GetAllGoalsAsync();
                return Ok(goals);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all goals");
                return StatusCode(500, new { message = "An error occurred while retrieving goals" });
            }
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveGoals()
        {
            try
            {
                var goals = await _goalService.GetActiveGoalsAsync();
                return Ok(goals);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active goals");
                return StatusCode(500, new { message = "An error occurred while retrieving active goals" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetGoalById(int id)
        {
            try
            {
                var goal = await _goalService.GetGoalByIdAsync(id);
                return Ok(goal);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting goal {GoalId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving goal" });
            }
        }

        [HttpGet("{id}/members")]
        public async Task<IActionResult> GetGoalMembers(int id)
        {
            try
            {
                var members = await _goalService.GetGoalMembersAsync(id);
                return Ok(members);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting members for goal {GoalId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving goal members" });
            }
        }

        [HttpGet("{id}/contributions")]
        public async Task<IActionResult> GetGoalContributions(int id)
        {
            try
            {
                var contributions = await _contributionService.GetContributionsByGoalAsync(id);
                return Ok(contributions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contributions for goal {GoalId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving goal contributions" });
            }
        }

        // Admin-only endpoints
        [HttpPost]
        public async Task<IActionResult> CreateGoal([FromBody] CreateGoalRequest request)
        {
            try
            {
                // Check if user is authenticated
                if (HttpContext.Items["AuthenticatedUser"] is not User user)
                {
                    _logger.LogWarning("CreateGoal: User not authenticated");
                    return Unauthorized(new { message = "User not authenticated. Please provide X-User-Email and X-User-Password headers." });
                }
                
                // Only Admin can create goals
                if (user.Role != "Admin")
                {
                    _logger.LogWarning("CreateGoal: User {UserId} with role {Role} attempted to create goal", user.UserId, user.Role);
                    return Forbid();
                }

                var goal = await _goalService.CreateGoalAsync(request, user.UserId);

                _logger.LogInformation("Admin {AdminId} created goal {GoalId}: {GoalName}",
                    user.UserId, goal.GoalId, goal.GoalName);

                return CreatedAtAction(nameof(GetGoalById), new { id = goal.GoalId }, goal);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("CreateGoal: UnauthorizedAccessException - {Message}", ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating goal: {Message}", ex.Message);
                return StatusCode(500, new { message = "An error occurred while creating goal" });
            }
        }

       

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGoal(int id, [FromBody] UpdateGoalRequest request)
        {
            try
            {
                // Check if user is authenticated
                if (HttpContext.Items["AuthenticatedUser"] is not User user)
                {
                    _logger.LogWarning("UpdateGoal: User not authenticated");
                    return Unauthorized(new { message = "User not authenticated. Please provide X-User-Email and X-User-Password headers." });
                }
                
                // Only Admin can update goals
                if (user.Role != "Admin")
                {
                    _logger.LogWarning("UpdateGoal: User {UserId} with role {Role} attempted to update goal", user.UserId, user.Role);
                    return Forbid();
                }

                // Validate request has at least one field to update
                if (string.IsNullOrEmpty(request.GoalName) && 
                    string.IsNullOrEmpty(request.Description) && 
                    !request.TargetAmount.HasValue && 
                    !request.EndDate.HasValue && 
                    string.IsNullOrEmpty(request.Status))
                {
                    return BadRequest(new { message = "At least one field must be provided for update" });
                }

                var goal = await _goalService.UpdateGoalAsync(id, request);

                _logger.LogInformation("Goal {GoalId} updated", id);

                return Ok(goal);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating goal {GoalId}: {Message}", id, ex.Message);
                return StatusCode(500, new { message = $"An error occurred while updating goal: {ex.Message}" });
            }
        }

        [HttpDelete("{id}")]
       // [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteGoal(int id)
        {
            try
            {
                var result = await _goalService.DeleteGoalAsync(id);

                if (result)
                {
                    _logger.LogInformation("Goal {GoalId} deleted", id);
                    return Ok(new { message = "Goal deleted successfully" });
                }

                return BadRequest(new { message = "Failed to delete goal" });
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
                _logger.LogError(ex, "Error deleting goal {GoalId}", id);
                return StatusCode(500, new { message = "An error occurred while deleting goal" });
            }
        }

        [HttpPut("{id}/update-progress")]
       // [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateGoalProgress(int id)
        {
            try
            {
                var goal = await _goalService.UpdateGoalProgressAsync(id);

                _logger.LogInformation("Goal {GoalId} progress updated", id);

                return Ok(goal);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating progress for goal {GoalId}", id);
                return StatusCode(500, new { message = "An error occurred while updating goal progress" });
            }
        }

        [HttpGet("stats")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetGoalStats()
        {
            try
            {
                var stats = await _goalService.GetGoalStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting goal stats");
                return StatusCode(500, new { message = "An error occurred while retrieving goal stats" });
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

        private User GetAuthenticatedUser()
        {
            // Try to get from AuthenticatedUser
            if (HttpContext.Items["AuthenticatedUser"] is User user)
                return user;
            
            throw new UnauthorizedAccessException("User not authenticated");
        }
    }
}