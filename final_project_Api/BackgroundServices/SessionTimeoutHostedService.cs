using final_project_Core.Interface;
using Microsoft.Extensions.Options;

namespace final_project_API.BackgroundServices
{
    // Periodically ends charging sessions that have been active longer than the
    // configured max duration, freeing their spot back to Available.
    public class SessionTimeoutHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<SessionTimeoutHostedService> _logger;
        private readonly SessionTimeoutOptions _options;

        public SessionTimeoutHostedService(
            IServiceScopeFactory scopeFactory,
            IOptions<SessionTimeoutOptions> options,
            ILogger<SessionTimeoutHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _options = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var maxDuration = TimeSpan.FromMinutes(_options.MaxDurationMinutes);
            var checkInterval = TimeSpan.FromMinutes(_options.CheckIntervalMinutes);

            using var timer = new PeriodicTimer(checkInterval);
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var chargingSpotService = scope.ServiceProvider.GetRequiredService<IChargingSpotService>();

                    var endedCount = await chargingSpotService.EndExpiredSessionsAsync(maxDuration, stoppingToken);
                    if (endedCount > 0)
                    {
                        _logger.LogInformation("Session timeout sweep auto-ended {Count} session(s)", endedCount);
                    }
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Session timeout sweep failed");
                }

                try
                {
                    await timer.WaitForNextTickAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }
}
