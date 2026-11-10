using Microsoft.EntityFrameworkCore;
using SchoolAdmission.Domain.Enums;
using SchoolAdmission.Infrastructure.Persistence;

namespace SchoolAdmission.Api.Workers;

public class ExpiredHoldCleaner(IServiceScopeFactory scopeFactory, ILogger<ExpiredHoldCleaner> logger)
    : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);

        do
        {
            try
            {
                // the worker lives for the whole app lifetime, but the DbContext is scoped,
                // so each run creates its own scope
                using var scope = scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<SchoolAdmissionDbContext>();
                var now = DateTime.UtcNow;

                var count = await db.Holds
                    .Where(h => h.Status == HoldStatus.Held && h.ExpiresAt <= now)
                    .ExecuteUpdateAsync(s => s.SetProperty(h => h.Status, HoldStatus.Expired), stoppingToken);

                if (count > 0)
                    logger.LogInformation("Marked {Count} expired hold(s).", count);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // a failed run must not kill the worker; the next tick tries again
                logger.LogError(ex, "Failed to mark expired holds.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}