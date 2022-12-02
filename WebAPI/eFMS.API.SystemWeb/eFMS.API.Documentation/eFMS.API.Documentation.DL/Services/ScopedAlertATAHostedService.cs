using eFMS.API.Common.Helpers;
using eFMS.API.Documentation.DL.IService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace eFMS.API.Documentation.DL.Services
{
    public class ScopedAlertHostedService : BackgroundService
    {
        public IServiceScopeFactory services { get; }
        private readonly ILogger<IScopedProcessingAlertService> logger;
        public ScopedAlertHostedService(IServiceScopeFactory _service, ILogger<IScopedProcessingAlertService> _log)
        {
            services = _service;
            logger = _log;
        }
      
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            do
            {
                int hourSpan = 24 - DateTime.Now.Hour;
                new LogHelper(string.Format("ScopedAlerHostedService"), DateTime.Now + "\n");
                int numberOfHours = hourSpan;

                if (hourSpan == 12)
                {
                    using (var scope = services.CreateScope())
                    {
                        var scopedProcessingService = scope.ServiceProvider.GetRequiredService<IScopedProcessingAlertService>();
                        var dataAtd = scopedProcessingService.GetAlertATDData();
                        var dataAta = scopedProcessingService.GetAlertATAData();

                        scopedProcessingService.AlertATD();
                        scopedProcessingService.AlertATA();
                        new LogHelper("ScopedAlertATDHostedService Atd " + hourSpan, JsonConvert.SerializeObject(dataAtd));
                        new LogHelper("ScopedAlertATAHostedService Ata " + hourSpan, JsonConvert.SerializeObject(dataAta));
                    }
                    new LogHelper(string.Format("ScopedAlertHostedService"), "Delay " + hourSpan.ToString() + " To " + DateTime.Now.AddHours(hourSpan) + "\n");
                    numberOfHours = 12;
                }

                await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
            }
            while (!stoppingToken.IsCancellationRequested);
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Alert Service Hosted Service is stopping.");
            new LogHelper("ScopedAlertHostedService", "STOPPED\n");
            await base.StopAsync(stoppingToken);
        }
    }
}
