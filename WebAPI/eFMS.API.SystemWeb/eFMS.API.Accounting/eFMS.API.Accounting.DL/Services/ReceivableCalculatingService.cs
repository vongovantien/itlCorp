﻿using eFMS.API.Accounting.DL.IService;
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
                TimeSpan interval = TimeSpan.FromSeconds(45);
                new LogHelper("ReceivableCalculatingBackgroundService", "RUNNING at " + DateTime.Now);
                await _busControl.ReceiveAsync<List<ObjectReceivableModel>>(RabbitExchange.EFMS_Accounting, RabbitConstants.CalculatingReceivableDataPartnerQueue, async (models) =>
                {
                    Console.WriteLine("==================== ReceivableCalculatingBackgroundService ============================");
                    new LogHelper("ReceivableCalculatingBackgroundService", "EXCUTE at " + DateTime.Now + " " + JsonConvert.SerializeObject(models));
                    using (var scope = _services.CreateScope())
                    {
                        var scopedService = scope.ServiceProvider.GetRequiredService<IAccAccountReceivableHostedService>();
                        var d = await scopedService.CalculatorReceivableDebitAmountAsync(models);
                    }
                    Console.WriteLine("==================== ReceivableCalculatingBackgroundService ============================");

                }, batchSize: 3, maxMessagesInFlight: 10);
            }
            catch (Exception ex)
            {
                new LogHelper("ReceivableCalculatingBackgroundService", " ERROR at " + DateTime.Now + " " + ex.ToString() + " ");
                throw;
            }
            
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            try
            {
                new LogHelper("ReceivableCalculatingBackgroundService", "STOPPED at " + DateTime.Now);
                await base.StopAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                new LogHelper("ReceivableCalculatingBackgroundService", " ERROR at " + DateTime.Now + " " + ex.ToString() + " ");
                throw;
            }
           
        }
    }
    
}
