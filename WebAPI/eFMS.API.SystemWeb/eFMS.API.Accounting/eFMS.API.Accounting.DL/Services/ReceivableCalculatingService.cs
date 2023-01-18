using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Common.Helpers;
using eFMS.API.Infrastructure.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace eFMS.API.Accounting.DL.Services
{
    public class ReceivableCalculatingBackgroundService : BackgroundService
    {
        private readonly IRabbitBus _busControl;
        public IServiceScopeFactory _services { get; }
        public ReceivableCalculatingBackgroundService(IRabbitBus busControl, IServiceScopeFactory service)
        {
            _busControl = busControl;
            _services = service;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _busControl.ReceiveAsync<List<ObjectReceivableModel>>(RabbitConstants.CalculatingReceivableDataPartnerQueue, (models) => 
            {
                Console.WriteLine("==================== ReceivableCalculatingBackgroundService ============================");
                
                using (var scope = _services.CreateScope())
                {
                    var scopedService = scope.ServiceProvider.GetRequiredService<IAccAccountReceivableHostedService>();
                    scopedService.CalculatorReceivableDebitAmountAsync(models);
                }
                Console.WriteLine("==================== ReceivableCalculatingBackgroundService ============================");

            });
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            new LogHelper("ReceivableCalculatingBackgroundService", "STOPPED\n");
            await base.StopAsync(stoppingToken);
        }
    }
    
}
