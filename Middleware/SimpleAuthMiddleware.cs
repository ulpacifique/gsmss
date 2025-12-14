using System.Net;
using System.Text.Json;
using CommunityFinanceAPI.Models.Entities;
using CommunityFinanceAPI.Services.Interfaces;

namespace CommunityFinanceAPI.Middleware
{
    public class SimpleAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SimpleAuthMiddleware> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public SimpleAuthMiddleware(
            RequestDelegate next,
            ILogger<SimpleAuthMiddleware> logger,
            IServiceScopeFactory serviceScopeFactory)
        {
            _next = next;
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value ?? "";
            var method = context.Request.Method;
            
            // #region agent log
            try {
                System.IO.File.AppendAllText(@"d:\Dotnet\CommunityFinanceAPI\.cursor\debug.log", 
                    System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "E", location = "SimpleAuthMiddleware.InvokeAsync:24", message = "Middleware invoked", data = new { path = path, method = method, isPublicEndpoint = IsPublicEndpoint(path), isOptions = method == "OPTIONS" }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n");
            } catch { }
            // #endregion
            
            // Skip auth for public endpoints and OPTIONS requests (CORS preflight)
            _logger.LogInformation("🔵 SimpleAuthMiddleware: Path={Path}, Method={Method}", path, method);
            
            if (IsPublicEndpoint(path) || method == "OPTIONS")
            {
                _logger.LogInformation("✅ Skipping auth for public endpoint: {Path}", path);
                // #region agent log
                try {
                    System.IO.File.AppendAllText(@"d:\Dotnet\CommunityFinanceAPI\.cursor\debug.log", 
                        System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "E", location = "SimpleAuthMiddleware.InvokeAsync:36", message = "Skipping auth for public endpoint", data = new { path = path }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n");
                } catch { }
                // #endregion
                await _next(context);
                return;
            }

            // Check for credentials in headers
            var hasEmailHeader = context.Request.Headers.TryGetValue("X-User-Email", out var emailHeader);
            var hasPasswordHeader = context.Request.Headers.TryGetValue("X-User-Password", out var passwordHeader);
            
            _logger.LogInformation("=== AUTH CHECK for: {Path} ===", path);
            _logger.LogInformation("Has X-User-Email: {HasEmail}, Has X-User-Password: {HasPassword}", hasEmailHeader, hasPasswordHeader);
            
            // #region agent log
            try {
                System.IO.File.AppendAllText(@"d:\Dotnet\CommunityFinanceAPI\.cursor\debug.log", 
                    System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "D", location = "SimpleAuthMiddleware.InvokeAsync:59", message = "Checking headers", data = new { path = path, hasEmailHeader = hasEmailHeader, hasPasswordHeader = hasPasswordHeader, emailValue = hasEmailHeader ? emailHeader.ToString().Substring(0, Math.Min(20, emailHeader.ToString().Length)) : "none", passwordLength = hasPasswordHeader ? passwordHeader.ToString().Length : 0 }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n");
            } catch { }
            // #endregion
            
            if (hasEmailHeader && hasPasswordHeader)
            {
                var email = emailHeader.ToString().Trim();
                var password = passwordHeader.ToString().Trim();
                
                // Validate headers are not empty
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    _logger.LogWarning("❌ Empty email or password headers for path: {Path}", path);
                    await SendUnauthorizedResponse(context);
                    return;
                }

                _logger.LogInformation("Email: {Email} (password length: {PasswordLength})", email, password?.Length ?? 0);

                try
                {
                    // Validate user using service
                    using var scope = _serviceScopeFactory.CreateScope();
                    var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

                    // Debug: Print what we're checking
                    _logger.LogInformation("=== VALIDATION START ===");
                    _logger.LogInformation("Attempting to validate credentials for: {Email}", email);

                    var isValid = await authService.ValidateUserCredentialsAsync(email, password);

                    _logger.LogInformation("Validation result: {Result}", isValid);
                    _logger.LogInformation("=== END VALIDATION ===");
                    
                    // If validation failed, log more details
                    if (!isValid)
                    {
                        _logger.LogWarning("⚠️ Validation returned FALSE for email: {Email}", email);
                        // Try to get user to see if it exists
                        var testUser = await authService.GetUserByEmailAsync(email);
                        if (testUser == null)
                        {
                            _logger.LogWarning("⚠️ User not found in database: {Email}", email);
                        }
                        else
                        {
                            _logger.LogWarning("⚠️ User exists but password verification failed: {Email}", email);
                        }
                    }

                    if (isValid)
                    {
                        // Get user info and store in context for this request
                        var user = await authService.GetUserByEmailAsync(email);
                        if (user != null)
                        {
                            // Store user in HttpContext for this request
                            context.Items["AuthenticatedUser"] = user;
                            context.Items["UserId"] = user.UserId;
                            context.Items["UserRole"] = user.Role;

                            _logger.LogInformation("✅ User authenticated: {Email} (Role: {Role}, UserId: {UserId})", user.Email, user.Role, user.UserId);
                            _logger.LogInformation("✅ HttpContext.Items now contains: {Keys}", string.Join(", ", context.Items.Keys));
                            
                            // #region agent log
                            try {
                                System.IO.File.AppendAllText(@"d:\Dotnet\CommunityFinanceAPI\.cursor\debug.log", 
                                    System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "D", location = "SimpleAuthMiddleware.InvokeAsync:87", message = "User authenticated and stored in context", data = new { email = user.Email, userId = user.UserId, role = user.Role, contextItemsKeys = string.Join(", ", context.Items.Keys) }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n");
                            } catch { /* Ignore file lock errors */ }
                            // #endregion
                            
                            await _next(context);
                            return;
                        }
                        else
                        {
                            _logger.LogError("❌ User not found after validation: {Email}", email);
                            await SendUnauthorizedResponse(context);
                            return;
                        }
                    }
                    else
                    {
                        _logger.LogWarning("❌ Invalid credentials for: {Email} - Password verification failed", email);
                        // #region agent log
                        try {
                            System.IO.File.AppendAllText(@"d:\Dotnet\CommunityFinanceAPI\.cursor\debug.log", 
                                System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "E", location = "SimpleAuthMiddleware.InvokeAsync:112", message = "Validation failed", data = new { email = email, path = path }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n");
                        } catch { /* Ignore file lock errors */ }
                        // #endregion
                        await SendUnauthorizedResponse(context);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error validating user credentials for {Email}: {Message}", email, ex.Message);
                    await SendUnauthorizedResponse(context);
                    return;
                }
            }
            else
            {
                _logger.LogWarning("❌ Missing X-User-Email or X-User-Password headers for path: {Path}", path);
                _logger.LogWarning("Available headers: {Headers}", string.Join(", ", context.Request.Headers.Keys));
                _logger.LogWarning("=== AUTH FAILED ===");
                // #region agent log
                try {
                    System.IO.File.AppendAllText(@"d:\Dotnet\CommunityFinanceAPI\.cursor\debug.log", 
                        System.Text.Json.JsonSerializer.Serialize(new { sessionId = "debug-session", runId = "run1", hypothesisId = "E", location = "SimpleAuthMiddleware.InvokeAsync:126", message = "Missing headers", data = new { path = path, hasEmailHeader = hasEmailHeader, hasPasswordHeader = hasPasswordHeader, availableHeaders = string.Join(", ", context.Request.Headers.Keys) }, timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }) + "\n");
                } catch { /* Ignore file lock errors */ }
                // #endregion
                await SendUnauthorizedResponse(context);
                return; // Stop the pipeline
            }

            // If we get here, authentication failed
            await SendUnauthorizedResponse(context);
            return; // Stop the pipeline
        }

        private bool IsPublicEndpoint(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;
                
            // Normalize path to lowercase for comparison
            var normalizedPath = path.ToLowerInvariant();
            
            // List of public endpoints that don't require authentication
            // NOTE: Don't include "/" as it matches everything!
            var publicEndpoints = new[]
            {
                "/api/auth/login",
                "/api/auth/register",
                "/swagger",
                "/favicon.ico"
            };

            // Check if path starts with any public endpoint (case-insensitive)
            // Use exact match for root path, or starts with for others
            if (normalizedPath == "/")
                return true;
                
            return publicEndpoints.Any(endpoint => normalizedPath.StartsWith(endpoint.ToLowerInvariant()));
        }

        private async Task SendUnauthorizedResponse(HttpContext context)
        {
            // Don't write response if it has already started
            if (context.Response.HasStarted)
            {
                _logger.LogWarning("Response has already started, cannot send unauthorized response");
                return;
            }

            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            context.Response.ContentType = "application/json";

            var errorResponse = new
            {
                StatusCode = 401,
                Message = "Unauthorized. Please provide valid X-User-Email and X-User-Password headers.",
                Path = context.Request.Path,
                RequiredHeaders = new[] { "X-User-Email", "X-User-Password" }
            };

            var result = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(result);
            // Don't call CompleteAsync() - let the pipeline handle it naturally
        }
    }

    public static class SimpleAuthMiddlewareExtensions
    {
        public static IApplicationBuilder UseSimpleAuthMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SimpleAuthMiddleware>();
        }
    }
}