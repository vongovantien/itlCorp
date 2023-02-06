using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Common.Helpers;
using eFMS.API.Infrastructure.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
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
            try
            {
                new LogHelper("ReceivableCalculatingBackgroundService", "RUNNING\n");
                await _busControl.ReceiveAsync<List<ObjectReceivableModel>>(RabbitExchange.EFMS_Accounting, RabbitConstants.CalculatingReceivableDataPartnerQueue, (models) =>
                {
                    Console.WriteLine("==================== ReceivableCalculatingBackgroundService ============================");
                    new LogHelper("ReceivableCalculatingBackgroundService", "EXCUTE\n" + JsonConvert.SerializeObject(models));
                    using (var scope = _services.CreateScope())
                    {
                        var scopedService = scope.ServiceProvider.GetRequiredService<IAccAccountReceivableHostedService>();
                        scopedService.CalculatorReceivableDebitAmountAsync(models);
                    }
                    Console.WriteLine("==================== ReceivableCalculatingBackgroundService ============================");

                });
            }
            catch (Exception ex)
            {
                new LogHelper("ReceivableCalculatingBackgroundService", " ERROR\n" + ex.ToString() + " ");
                throw;
            }
            
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            try
            {
                new LogHelper("ReceivableCalculatingBackgroundService", "STOPPED\n");
                await base.StopAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                new LogHelper("ReceivableCalculatingBackgroundService", " ERROR\n" + ex.ToString() + " ");
                throw;
            }
           
        }
    }
    
}
