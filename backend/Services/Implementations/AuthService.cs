using CommunityFinanceAPI.Data;
using CommunityFinanceAPI.Models.DTOs;
using CommunityFinanceAPI.Models.Entities;
using CommunityFinanceAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CommunityFinanceAPI.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // === NEW METHODS FOR SIMPLE AUTHENTICATION ===
        public async Task<bool> ValidateUserCredentialsAsync(string email, string password)
        {
            Console.WriteLine($"=== VALIDATING CREDENTIALS ===");
            Console.WriteLine($"Email: {email}");
            Console.WriteLine($"Password length: {password?.Length ?? 0}");

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

            if (user == null)
            {
                Console.WriteLine("❌ User not found or inactive");
                return false;
            }

            Console.WriteLine($"✅ User found: {user.Email}");
            Console.WriteLine($"PasswordHash length: {user.PasswordHash?.Length ?? 0}");
            Console.WriteLine($"PasswordHash starts with: {user.PasswordHash?.Substring(0, Math.Min(10, user.PasswordHash?.Length ?? 0))}");

            if (string.IsNullOrWhiteSpace(password))
            {
                Console.WriteLine("❌ Password is null or empty");
                return false;
            }

            if (string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                Console.WriteLine("❌ PasswordHash is null or empty");
                return false;
            }

            var result = VerifyPassword(password, user.PasswordHash);
            Console.WriteLine($"Password match result: {result}");
            Console.WriteLine($"=== END VALIDATION ===");

            return result;
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
        }

        public async Task<UserResponse> GetUserResponseByEmailAsync(string email)
        {
            var user = await GetUserByEmailAsync(email);
            if (user == null)
                return null;

            return MapToUserResponse(user);
        }
        // === END NEW METHODS ===

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            // #region agent log
            try {
                System.IO.File.AppendAllText(@"d:\Dotnet\CommunityFinanceAPI\.cursor\debug.log", 
                    System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "B", location = "AuthService.LoginAsync:76", message = "LoginAsync called", data = new { email = request.Email }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n");
            } catch { /* Ignore file lock errors */ }
            // #endregion
            
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);
                
                // #region agent log
                try {
                    System.IO.File.AppendAllText(@"d:\Dotnet\CommunityFinanceAPI\.cursor\debug.log", 
                        System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "B", location = "AuthService.LoginAsync:82", message = "User query completed", data = new { email = request.Email, userFound = user != null, userId = user?.UserId }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n");
                } catch { /* Ignore file lock errors */ }
                // #endregion

                if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
                    throw new UnauthorizedAccessException("Invalid email or password");

                return new AuthResponse
                {
                    Token = null,
                    User = MapToUserResponse(user),
                    ExpiresAt = null
                };
            }
            catch (Exception ex)
            {
                // #region agent log
                try {
                    System.IO.File.AppendAllText(@"d:\Dotnet\CommunityFinanceAPI\.cursor\debug.log", 
                        System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "B", location = "AuthService.LoginAsync:catch", message = "LoginAsync exception", data = new { email = request.Email, error = ex.Message, innerException = ex.InnerException?.Message }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n");
                } catch { /* Ignore file lock errors */ }
                // #endregion
                throw;
            }
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            // #region agent log
            try {
                System.IO.File.AppendAllText(@"d:\Dotnet\CommunityFinanceAPI\.cursor\debug.log", 
                    System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "A", location = "AuthService.RegisterAsync:94", message = "RegisterAsync called", data = new { email = request.Email, hasFirstName = !string.IsNullOrEmpty(request.FirstName) }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n");
            } catch { /* Ignore file lock errors */ }
            // #endregion
            
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                throw new InvalidOperationException("Email already exists");

            // Determine role - if email contains "admin" or ends with "@admin.com", make them Admin
            // Otherwise, default to Member
            string role = "Member";
            if (!string.IsNullOrEmpty(request.Email) && 
                (request.Email.ToLower().Contains("admin") || 
                 request.Email.ToLower().EndsWith("@admin.com") ||
                 request.Email.ToLower().EndsWith("@community.com")))
            {
                role = "Admin";
            }

            var user = new User
            {
                Email = request.Email,
                PasswordHash = HashPassword(request.Password),
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                Role = role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            // #region agent log
            try {
                System.IO.File.AppendAllText(@"d:\Dotnet\CommunityFinanceAPI\.cursor\debug.log", 
                    System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "A", location = "AuthService.RegisterAsync:121", message = "User entity created", data = new { email = user.Email, hasProfilePictureUrl = user.ProfilePictureUrl != null, profilePictureUrlValue = user.ProfilePictureUrl }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n");
            } catch { /* Ignore file lock errors */ }
            // #endregion

            _context.Users.Add(user);
            
            // #region agent log
            try {
                System.IO.File.AppendAllText(@"d:\Dotnet\CommunityFinanceAPI\.cursor\debug.log", 
                    System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "A", location = "AuthService.RegisterAsync:125", message = "Before SaveChangesAsync", data = new { email = user.Email }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n");
            } catch { /* Ignore file lock errors */ }
            // #endregion
            
            try
            {
                await _context.SaveChangesAsync();
                
                // #region agent log
                try {
                    System.IO.File.AppendAllText(@"d:\Dotnet\CommunityFinanceAPI\.cursor\debug.log", 
                        System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "A", location = "AuthService.RegisterAsync:132", message = "SaveChangesAsync succeeded", data = new { email = user.Email, userId = user.UserId }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n");
                } catch { /* Ignore file lock errors */ }
                // #endregion
            }
            catch (Exception ex)
            {
                // #region agent log
                try {
                    System.IO.File.AppendAllText(@"d:\Dotnet\CommunityFinanceAPI\.cursor\debug.log", 
                        System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "A", location = "AuthService.RegisterAsync:137", message = "SaveChangesAsync failed", data = new { email = user.Email, error = ex.Message, innerException = ex.InnerException?.Message, stackTrace = ex.StackTrace != null ? ex.StackTrace.Substring(0, Math.Min(500, ex.StackTrace.Length)) : null }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n");
                } catch { /* Ignore file lock errors */ }
                // #endregion
                throw;
            }

            return new AuthResponse
            {
                Token = null,
                User = MapToUserResponse(user),
                ExpiresAt = null
            };
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            if (!VerifyPassword(request.CurrentPassword, user.PasswordHash))
                throw new UnauthorizedAccessException("Current password is incorrect");

            user.PasswordHash = HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ResetPasswordAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return false;

            var tempPassword = GenerateTemporaryPassword();
            user.PasswordHash = HashPassword(tempPassword);
            user.UpdatedAt = DateTime.UtcNow;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            // TODO: Send email with temporary password
            // await _emailService.SendPasswordResetEmail(user.Email, tempPassword);

            return true;
        }

        // UPDATED: Using BCrypt instead of SHA256
        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // UPDATED: Using BCrypt verification
        private bool VerifyPassword(string password, string passwordHash)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordHash))
                {
                    Console.WriteLine("❌ VerifyPassword: Password or hash is null/empty");
                    return false;
                }

                // Check if hash looks like BCrypt (starts with $2a$, $2b$, or $2y$)
                if (!passwordHash.StartsWith("$2") && !passwordHash.StartsWith("$2a$") && !passwordHash.StartsWith("$2b$") && !passwordHash.StartsWith("$2y$"))
                {
                    Console.WriteLine($"❌ VerifyPassword: PasswordHash doesn't look like BCrypt. Hash: {passwordHash.Substring(0, Math.Min(20, passwordHash.Length))}...");
                    // Try plain text comparison as fallback (for old passwords)
                    return password == passwordHash;
                }

                var result = BCrypt.Net.BCrypt.Verify(password, passwordHash);
                Console.WriteLine($"VerifyPassword result: {result}");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ VerifyPassword exception: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        private string GenerateTemporaryPassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private UserResponse MapToUserResponse(User user)
        {
            return new UserResponse
            {
                UserId = user.UserId,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                ProfilePictureUrl = user.ProfilePictureUrl,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }
    }
}