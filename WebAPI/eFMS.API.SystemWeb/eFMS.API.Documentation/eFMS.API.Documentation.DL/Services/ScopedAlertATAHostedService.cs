using eFMS.API.Common.Helpers;
using eFMS.API.Documentation.DL.IService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
                using (var scope = services.CreateScope())
                {
                    var scopedProcessingService =
                        scope.ServiceProvider
                            .GetRequiredService<IScopedProcessingAlertATDService>();
                    await scopedProcessingService.AlertATD();
                }
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            new LogHelper("ScopedAlertATDHostedService", "STOP\n");
            await base.StopAsync(stoppingToken);
        }
    }
}
