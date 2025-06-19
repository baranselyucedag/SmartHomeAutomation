using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;

namespace SmartHomeAutomation.API.Services
{
    public interface ILogService
    {
        void LogInformation(string message, params object[] args);
        void LogWarning(string message, params object[] args);
        void LogError(Exception ex, string message, params object[] args);
        void LogUserAction(ClaimsPrincipal user, string action, string details);
    }

    public class LogService : ILogService
    {
        private readonly ILogger<LogService> _logger;

        public LogService(ILogger<LogService> logger)
        {
            _logger = logger;
        }

        public void LogInformation(string message, params object[] args)
        {
            _logger.LogInformation(message, args);
        }

        public void LogWarning(string message, params object[] args)
        {
            _logger.LogWarning(message, args);
        }

        public void LogError(Exception ex, string message, params object[] args)
        {
            _logger.LogError(ex, message, args);
        }

        public void LogUserAction(ClaimsPrincipal user, string action, string details)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = user.FindFirst(ClaimTypes.Name)?.Value;

            _logger.LogInformation(
                "User Action: {Action} | User: {Username} (ID: {UserId}) | Details: {Details}",
                action,
                username,
                userId,
                details
            );
        }
    }
} 