using Application.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OnlineShop.Infrastructure.Services
{
    /// <summary>
    /// Wakes up periodically and asks the backup service to run the scheduled full backup when it is due.
    /// The schedule itself (enabled / hour / retention) is managed from the admin backup page.
    /// </summary>
    public class AutoBackupHostedService(
        IBackupService backupService,
        ILogger<AutoBackupHostedService> logger) : BackgroundService
    {
        private static readonly TimeSpan Tick = TimeSpan.FromMinutes(5);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                // Let the app finish starting (migrations, seeding) before the first check.
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        if (await backupService.RunScheduledIfDueAsync(stoppingToken))
                            logger.LogInformation("Scheduled full backup created");
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Scheduled backup failed");
                    }

                    await Task.Delay(Tick, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                // shutting down
            }
        }
    }
}
