using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CommunityFinanceAPI.Models.DTOs;
using CommunityFinanceAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommunityFinanceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var response = await _authService.LoginAsync(request);
                _logger.LogInformation("User {Email} logged in successfully", request.Email);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Failed login attempt for {Email}", request.Email);
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for {Email}", request.Email);
                return StatusCode(500, new { message = "An error occurred during login" });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var response = await _authService.RegisterAsync(request);
                _logger.LogInformation("User {Email} registered successfully", request.Email);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for {Email}", request.Email);
                return StatusCode(500, new { message = "An error occurred during registration" });
            }
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
                var result = await _authService.ChangePasswordAsync(userId, request);

                if (result)
                {
                    _logger.LogInformation("User {UserId} changed password successfully", userId);
                    return Ok(new { message = "Password changed successfully" });
                }

                return BadRequest(new { message = "Failed to change password" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}",
                    User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
                return StatusCode(500, new { message = "An error occurred while changing password" });
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                var result = await _authService.ResetPasswordAsync(request.Email);

                if (result)
                {
                    _logger.LogInformation("Password reset initiated for {Email}", request.Email);
                    return Ok(new { message = "If the email exists, a reset link has been sent" });
                }

                return BadRequest(new { message = "Failed to reset password" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for {Email}", request.Email);
                return StatusCode(500, new { message = "An error occurred while resetting password" });
            }
        }
        [HttpGet("debug")]
        [Authorize]
        public IActionResult DebugToken()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var name = User.FindFirst(ClaimTypes.Name)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            return Ok(new
            {
                Message = "Token debug information",
                IsAuthenticated = User.Identity?.IsAuthenticated,
                UserId = userId,
                Email = email,
                Name = name,
                Role = role,
                AllClaims = User.Claims.Select(c => new { Type = c.Type, Value = c.Value }).ToList()
            });
        }
        [HttpGet("test-claims")]
        [Authorize]
        public IActionResult TestClaims()
        {
            var claims = new
            {
                NameIdentifier = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                Email = User.FindFirst(ClaimTypes.Email)?.Value,
                Name = User.FindFirst(ClaimTypes.Name)?.Value,
                Role = User.FindFirst(ClaimTypes.Role)?.Value,
                Sub = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value,
                Jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value,
                AllClaims = User.Claims.Select(c => new { Type = c.Type, Value = c.Value }).ToList()
            };

            return Ok(claims);
        }
    }

    public class ResetPasswordRequest
    {
        public string Email { get; set; } = string.Empty;
    }

}