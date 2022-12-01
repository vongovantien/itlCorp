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
            do
            {
                int hourSpan = 24 - DateTime.Now.Hour;
                new LogHelper(string.Format("ScopedAlertATAHostedService"), hourSpan.ToString() + "\n");
                int numberOfHours = hourSpan;

                if (hourSpan == 24)
                {
                    new LogHelper(string.Format("ScopedAlertATAHostedService", "Alert Service Hosted Service is excuted " + DateTime.Now));
                    using (var scope = services.CreateScope())
                    {
                        var scopedProcessingService = scope.ServiceProvider.GetRequiredService<IScopedProcessingAlertATDService>();
                        var data = scopedProcessingService.GetAlertATDData();
                        new LogHelper("ScopedAlertATAHostedService" + hourSpan, JsonConvert.SerializeObject(data));
                    }
                    numberOfHours = 24;
                }
                new LogHelper(string.Format("ScopedAlertATAHostedService"), "Delay" + hourSpan.ToString() + "\n");
                await Task.Delay(TimeSpan.FromHours(numberOfHours), stoppingToken);
            }
            while (!stoppingToken.IsCancellationRequested);
        }

        private async Task DoWork(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                int hourSpan = 24 - DateTime.Now.Hour;
                int numberOfHours = hourSpan;

                if (hourSpan == 24)
                {
                    new LogHelper(string.Format("ScopedAlertATAHostedService", "Alert Service Hosted Service is excuted " + DateTime.Now));
                    using (var scope = services.CreateScope())
                    {
                        var scopedProcessingService = scope.ServiceProvider.GetRequiredService<IScopedProcessingAlertATDService>();
                        var data = scopedProcessingService.GetAlertATDData();
                        new LogHelper("ScopedAlertATAHostedService" + hourSpan, JsonConvert.SerializeObject(data));
                    }
                    numberOfHours = 24;
                }

                await Task.Delay(TimeSpan.FromHours(numberOfHours), stoppingToken);
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
