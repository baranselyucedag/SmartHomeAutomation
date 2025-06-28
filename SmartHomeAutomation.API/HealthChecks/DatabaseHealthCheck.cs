using Microsoft.Extensions.Diagnostics.HealthChecks;
using SmartHomeAutomation.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace SmartHomeAutomation.API.HealthChecks
{
    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly ApplicationDbContext _context;

        public DatabaseHealthCheck(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                // Try to connect to database and execute a simple query
                var canConnect = await _context.Database.CanConnectAsync(cancellationToken);
                
                if (canConnect)
                {
                    // Check if we can query users table
                    var userCount = await _context.Users.CountAsync(cancellationToken);
                    
                    return HealthCheckResult.Healthy($"Database is healthy. User count: {userCount}");
                }
                else
                {
                    return HealthCheckResult.Unhealthy("Cannot connect to database");
                }
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy($"Database health check failed: {ex.Message}");
            }
        }
    }
} 