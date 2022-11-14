using eFMS.API.Common.Helpers;
using eFMS.API.Documentation.DL.IService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace eFMS.API.Documentation.DL.Services
{
    public class ScopedAlertATDHostedService : BackgroundService
    {
        public IServiceProvider services { get; }
        private readonly ILogger<IScopedProcessingAlertATDService> logger;
        public ScopedAlertATDHostedService(IServiceProvider _service, ILogger<IScopedProcessingAlertATDService> _log)
        {
            services = _service;
            logger = _log;
        }
      
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            new LogHelper("ScopedAlertATAHostedService", "RUNNING\n");
            logger.LogInformation("Alert Service Hosted Service is running.");
            await DoWork(stoppingToken);
        }

        private async Task DoWork(CancellationToken stoppingToken)
        {
            logger.LogInformation("Alert Service Hosted Service is working.");
            new LogHelper("ScopedAlertATAHostedService", "WORKING\n");
            while (!stoppingToken.IsCancellationRequested)
            {
                new LogHelper("ScopedAlertATAHostedService", "now is: " + DateTime.Now.Hour);
                int hourCurrent = 25 - DateTime.Now.Hour;
                int numerOfHours = hourCurrent;
                new LogHelper("ScopedAlertATAHostedService", "hourCurrent: " + numerOfHours);
                using (var scope = services.CreateScope())
                {
                    new LogHelper("ScopedAlertATAHostedService", "Alert Service Hosted Service is excuted - {0}" + DateTime.Now);
                    var scopedProcessingService =
                        scope.ServiceProvider
                            .GetRequiredService<IScopedProcessingAlertATDService>();
                    await scopedProcessingService.AlertATD();
                }
                await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Alert Service Hosted Service is stopping.");
            new LogHelper("ScopedAlertATDHostedService", "STOPPED\n");
            await base.StopAsync(stoppingToken);
        }
    }
}
