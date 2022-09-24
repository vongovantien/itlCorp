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
        public IServiceScopeFactory services { get; }
        private readonly ILogger<IScopedProcessingAlertATDService> logger;
        public ScopedAlertATDHostedService(IServiceScopeFactory _service, ILogger<IScopedProcessingAlertATDService> _log)
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
            while (!stoppingToken.IsCancellationRequested)
            {
                int hourCurrent = 25 - DateTime.Now.Hour;
                int numerOfHours = hourCurrent;
                if(numerOfHours == 24)
                {
                    using (var scope = services.CreateScope())
                    {
                        new LogHelper("ScopedAlertATAHostedService", "Alert Service Hosted Service is excuted - {0}" + DateTime.Now);
                        var scopedProcessingService =
                            scope.ServiceProvider
                                .GetRequiredService<IScopedProcessingAlertATDService>();
                        await scopedProcessingService.AlertATD();
                    }
                    numerOfHours = 24;
                }
                await Task.Delay(TimeSpan.FromHours(numerOfHours), stoppingToken);
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
