using System.Security.Cryptography;
using System.Text;

namespace SmartHomeAutomation.API.Middleware
{
    public class CsrfProtectionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CsrfProtectionMiddleware> _logger;
        private const string CsrfTokenHeader = "X-CSRF-Token";
        private const string CsrfTokenCookie = "CSRF-Token";

        public CsrfProtectionMiddleware(RequestDelegate next, ILogger<CsrfProtectionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip CSRF protection for GET, HEAD, OPTIONS, and API endpoints with JWT auth
            if (IsReadOnlyRequest(context.Request.Method) || IsApiEndpoint(context.Request.Path))
            {
                await _next(context);
                return;
            }

            // Generate CSRF token for first visit
            if (!context.Request.Cookies.ContainsKey(CsrfTokenCookie))
            {
                var token = GenerateCsrfToken();
                context.Response.Cookies.Append(CsrfTokenCookie, token, new CookieOptions
                {
                    HttpOnly = false, // JavaScript needs to read this
                    Secure = !context.Request.IsHttps ? false : true,
                    SameSite = SameSiteMode.Strict,
                    MaxAge = TimeSpan.FromHours(1)
                });
            }

            // Validate CSRF token for state-changing requests
            if (IsStateChangingRequest(context.Request.Method))
            {
                var cookieToken = context.Request.Cookies[CsrfTokenCookie];
                var headerToken = context.Request.Headers[CsrfTokenHeader].FirstOrDefault();

                if (string.IsNullOrEmpty(cookieToken) || string.IsNullOrEmpty(headerToken) || cookieToken != headerToken)
                {
                    _logger.LogWarning("CSRF token validation failed for {Method} {Path}", 
                        context.Request.Method, context.Request.Path);
                    
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsync("CSRF token validation failed");
                    return;
                }
            }

            await _next(context);
        }

        private static bool IsReadOnlyRequest(string method)
        {
            return method.Equals("GET", StringComparison.OrdinalIgnoreCase) ||
                   method.Equals("HEAD", StringComparison.OrdinalIgnoreCase) ||
                   method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsStateChangingRequest(string method)
        {
            return method.Equals("POST", StringComparison.OrdinalIgnoreCase) ||
                   method.Equals("PUT", StringComparison.OrdinalIgnoreCase) ||
                   method.Equals("DELETE", StringComparison.OrdinalIgnoreCase) ||
                   method.Equals("PATCH", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsApiEndpoint(string path)
        {
            return path.StartsWith("/api", StringComparison.OrdinalIgnoreCase);
        }

        private static string GenerateCsrfToken()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[32];
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }
    }
} 