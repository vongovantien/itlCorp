using eFMS.API.Common.Helpers;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace eFMS.API.Documentation.DL.Services
{
    public class ScopedAlertATAHostedService : BackgroundService, IHostedService
    {
        public IServiceScopeFactory services { get; }
        public ScopedAlertATAHostedService(IServiceScopeFactory _service)
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
                            .GetRequiredService<IScopedProcessingAlertATAService>();
                    //var csTransaction =
                    //    scope.ServiceProvider
                    //        .GetRequiredService<ICsTransactionScoped>();
                    await scopedProcessingService.AlertATA();
                }
            }
            
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            new LogHelper("ScopedAlertATAHostedService", "STOP\n");
            await base.StopAsync(stoppingToken);
        }
    }
}
