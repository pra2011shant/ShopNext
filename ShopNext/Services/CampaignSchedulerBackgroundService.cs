using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ShopNext.Services
{
    /// <summary>
    /// Point 51: Scheduled Sale Automated Lifecycle Background Service
    /// Periodically evaluates scheduled start times and end times:
    /// - Start time reached -> Campaign auto-transitions to ACTIVE / LIVE NOW
    /// - End time reached -> Campaign auto-transitions to EXPIRED
    /// Operates autonomously without requiring manual admin intervention.
    /// </summary>
    public class CampaignSchedulerBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CampaignSchedulerBackgroundService> _logger;

        public CampaignSchedulerBackgroundService(IServiceProvider serviceProvider, ILogger<CampaignSchedulerBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Point 51 CampaignSchedulerBackgroundService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var campaignService = scope.ServiceProvider.GetRequiredService<ISaleCampaignService>();
                        int transitions = await campaignService.AutoSyncCampaignLifecyclesAsync();
                        if (transitions > 0)
                        {
                            _logger.LogInformation("Point 51 Auto-Scheduler processed {TransitionsCount} campaign lifecycle transitions.", transitions);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in CampaignSchedulerBackgroundService.");
                }

                // Check every 30 seconds
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }

            _logger.LogInformation("Point 51 CampaignSchedulerBackgroundService stopped.");
        }
    }
}
