using eFMS.API.Common.Helpers;
using eFMS.API.Documentation.DL.IService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace eFMS.API.Documentation.DL.Services
{
    public class ScopedAlertATDHostedService : BackgroundService
    {
        public IServiceScopeFactory services { get; }
        public ScopedAlertATDHostedService(IServiceScopeFactory _service)
        {
            services = _service;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            new LogHelper("ScopedAlertATAHostedService", "RUNIING\n");
            await DoWork(stoppingToken);
        }

        private async Task DoWork(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                //int currentHour = 25 - DateTime.Now.Hour;
                //int numberHours = currentHour;
                //if(numberHours == 8)
                using (var scope = services.CreateScope())
                {
                    var scopedProcessingService =
                        scope.ServiceProvider
                            .GetRequiredService<IScopedProcessingAlertATDService>();
                    await scopedProcessingService.AlertATD();
                }
                await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            new LogHelper("ScopedAlertATDHostedService", "STOP\n");
            await base.StopAsync(stoppingToken);
        }
    }
}
