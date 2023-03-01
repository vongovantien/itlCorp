using Cronos;
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
            while (!stoppingToken.IsCancellationRequested)
            {
                await WaitForNextSchedule("0 8 * * 1-5"); // At 8:00am on Monday to Friday
                using (var scope = services.CreateScope())
                {
                    var scopedProcessingService = scope.ServiceProvider.GetRequiredService<IScopedProcessingAlertService>();
                    var dataAtd = scopedProcessingService.GetAlertATDData();
                    var dataAta = scopedProcessingService.GetAlertATAData();

                    scopedProcessingService.AlertATD();
                    scopedProcessingService.AlertATA();
                    new LogHelper("ScopedAlertATDHostedService Atd ", JsonConvert.SerializeObject(dataAtd));
                    new LogHelper("ScopedAlertATAHostedService Ata ", JsonConvert.SerializeObject(dataAta));
                }
            }
        }

        private async Task WaitForNextSchedule(string cronExpression)
        {
            var parsedExp = CronExpression.Parse(cronExpression);
            var currentTime = DateTimeOffset.Now.DateTime;
            var occurenceTime = parsedExp.GetNextOccurrence(DateTimeOffset.Now, TimeZoneInfo.Local);

            var delay = occurenceTime.GetValueOrDefault() - currentTime;
            new LogHelper(string.Format("ScopedAlertHostedService"), "Delay " + delay + "\n");

            await Task.Delay(delay);
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Alert Service Hosted Service is stopping.");
            new LogHelper("ScopedAlertHostedService", "STOPPED\n");
            await base.StopAsync(stoppingToken);
        }
    }
}
