using CommunityFinanceAPI.Models.DTOs;
using CommunityFinanceAPI.Models.Entities;

namespace CommunityFinanceAPI.Services.Interfaces
{
    public interface IAuthService
    {
        // Simple Authentication Methods - ADD THESE
        Task<bool> ValidateUserCredentialsAsync(string email, string password);
        Task<User> GetUserByEmailAsync(string email);
        Task<UserResponse> GetUserResponseByEmailAsync(string email);

        // Existing Methods (JWT-based - optional)
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request);
        Task<bool> ResetPasswordAsync(string email);
    }
}