using eFMS.API.Common.Helpers;
using eFMS.API.Infrastructure.RabbitMQ;
using eFMS.API.SystemFileManagement.DL.IService;
using eFMS.API.SystemFileManagement.DL.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace eFMS.API.SystemFileManagement.DL.Services
{
    public class EDocBackgroundService:BackgroundService
    {
        private readonly IRabbitBus _busControl;
        public IServiceScopeFactory _services { get; }
        public EDocBackgroundService(IRabbitBus busControl, IServiceScopeFactory service)
        {
            _busControl = busControl;
            _services = service;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                TimeSpan interval = TimeSpan.FromSeconds(45);
                new LogHelper("EDocBackgroundService", "RUNNING at " + DateTime.Now);
                await _busControl.ReceiveAsync<FileUploadAttachTemplateModel>(RabbitExchange.EFMS_FileManagement, RabbitConstants.PostAttachFileTemplateToEDocQueue, async (models) =>
                {
                    Console.WriteLine("==================== EDocBackgroundService ============================");
                    new LogHelper("EDocBackgroundService", "EXCUTE at " + DateTime.Now + " " + JsonConvert.SerializeObject(models));
                    using (var scope = _services.CreateScope())
                    {
                        var scopedService = scope.ServiceProvider.GetRequiredService<IEDocService>();
                        var d = await scopedService.PostAttachFileTemplateToEDoc(models);
                    }
                    Console.WriteLine("==================== EDocBackgroundService ============================");

                }, batchSize: 3, maxMessagesInFlight: 10);
            }
            catch (Exception ex)
            {
                new LogHelper("EDocBackgroundService", " ERROR at " + DateTime.Now + " " + ex.ToString() + " ");
                throw;
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            try
            {
                new LogHelper("EDocBackgroundService", "STOPPED at " + DateTime.Now);
                await base.StopAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                new LogHelper("EDocBackgroundService", " ERROR at " + DateTime.Now + " " + ex.ToString() + " ");
                throw;
            }

        }
    }
}
