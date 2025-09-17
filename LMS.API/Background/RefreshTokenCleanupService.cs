using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.API.Background;
/// <summary>
/// Periodically removes expired refresh tokens older than a retention window.
/// </summary>
public class RefreshTokenCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RefreshTokenCleanupService> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromHours(6);
    private static readonly TimeSpan Retention = TimeSpan.FromDays(7); // keep a week for forensic review
    public RefreshTokenCleanupService(IServiceProvider serviceProvider, ILogger<RefreshTokenCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var threshold = DateTime.UtcNow - Retention;
                var toDelete = await db.RefreshTokens
                    .Where(r => r.Expires < threshold)
                    .ExecuteDeleteAsync(stoppingToken);
                if (toDelete > 0)
                    _logger.LogInformation("RefreshTokenCleanup removed {Count} expired tokens", toDelete);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during refresh token cleanup");
            }
            await Task.Delay(Interval, stoppingToken);
        }
    }
}
