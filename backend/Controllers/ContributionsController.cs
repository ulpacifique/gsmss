using CommunityFinanceAPI.Models.DTOs;
using CommunityFinanceAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommunityFinanceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ContributionsController : ControllerBase
    {
        private readonly IContributionService _contributionService;
        private readonly ILogger<ContributionsController> _logger;

        public ContributionsController(IContributionService contributionService, ILogger<ContributionsController> logger)
        {
            _contributionService = contributionService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetContributionById(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
                var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)!.Value;

                var contribution = await _contributionService.GetContributionByIdAsync(id);

                // Members can only view their own contributions
                if (userRole == "Member" && contribution.UserId != userId)
                    return Forbid();

                return Ok(contribution);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contribution {ContributionId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving contribution" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContribution(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
                var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)!.Value;

                var contribution = await _contributionService.GetContributionByIdAsync(id);

                // Members can only delete their own pending contributions
                if (userRole == "Member")
                {
                    if (contribution.UserId != userId)
                        return Forbid();

                    if (contribution.Status != "Pending")
                        return BadRequest(new { message = "Only pending contributions can be deleted" });
                }

                var result = await _contributionService.DeleteContributionAsync(id);

                if (result)
                {
                    _logger.LogInformation("Contribution {ContributionId} deleted by user {UserId}", id, userId);
                    return Ok(new { message = "Contribution deleted successfully" });
                }

                return BadRequest(new { message = "Failed to delete contribution" });
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
                _logger.LogError(ex, "Error deleting contribution {ContributionId}", id);
                return StatusCode(500, new { message = "An error occurred while deleting contribution" });
            }
        }

        [HttpGet("stats")]
        [Authorize(Roles = "Admin")]
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
    }
}