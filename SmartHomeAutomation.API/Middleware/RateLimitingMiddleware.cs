using System.Collections.Concurrent;
using System.Net;

namespace SmartHomeAutomation.API.Middleware
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private static readonly ConcurrentDictionary<string, ClientRequestInfo> _clients = new();
        private readonly int _maxRequests;
        private readonly TimeSpan _timeWindow;

        public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
            _maxRequests = 100; // 100 requests per minute
            _timeWindow = TimeSpan.FromMinutes(1);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var clientId = GetClientId(context);
            var now = DateTime.UtcNow;

            var clientInfo = _clients.GetOrAdd(clientId, new ClientRequestInfo());

            lock (clientInfo)
            {
                // Clean old requests
                clientInfo.RequestTimes = clientInfo.RequestTimes
                    .Where(time => now - time < _timeWindow)
                    .ToList();

                if (clientInfo.RequestTimes.Count >= _maxRequests)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                    context.Response.Headers["Retry-After"] = _timeWindow.TotalSeconds.ToString();
                    
                    _logger.LogWarning("Rate limit exceeded for client {ClientId}", clientId);
                    return;
                }

                clientInfo.RequestTimes.Add(now);
            }

            await _next(context);
        }

        private string GetClientId(HttpContext context)
        {
            // Try to get user ID from JWT token first
            var userId = context.User?.FindFirst("userId")?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                return $"user_{userId}";
            }

            // Fall back to IP address
            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }

    public class ClientRequestInfo
    {
        public List<DateTime> RequestTimes { get; set; } = new();
    }
} 