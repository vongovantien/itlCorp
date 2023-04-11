
using eFMS.API.Infrastructure.RabbitMQ;
using eFMS.API.ReportData.Consts;
using eFMS.API.ReportData.FormatExcel;
using eFMS.API.ReportData.HttpServices;
using eFMS.API.ReportData.Models.Accounting;
using eFMS.API.ReportData.Models;
using Microsoft.AspNetCore.Authentication.Twitter;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Office.Interop.Excel;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System.Net.Http;
using Swashbuckle.AspNetCore.Swagger;
using eFMS.API.ReportData.Helpers;

namespace eFMS.API.ReportData.Service.BackGroundServices
{
    public class GenFileSyncService: BackgroundService
    {
        private readonly IRabbitBus _busControl;
        public IServiceScopeFactory _services { get; }
        private readonly APIs aPis;
        public GenFileSyncService(IRabbitBus busControl, IServiceScopeFactory service, IOptions<APIs> appSettings)
        {
            _busControl = busControl;
            _services = service;
            aPis = appSettings.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                TimeSpan interval = TimeSpan.FromSeconds(45);
                await _busControl.ReceiveAsync<ExportDetailSettleModel>(RabbitExchange.EFMS_ReportData, RabbitConstants.GenFileQueue, async (models) =>
                {
                    var responseFromApi = await HttpServiceExtension.GetApi("https://localhost:44300/" + Urls.Accounting.DetailSettlementPaymentExportUrl + "?settlementId=" + models.SettlementId, models.AccessToken);
                    var dataObjects = responseFromApi.Content.ReadAsAsync<SettlementExport>();
                    var stream = new AccountingHelper().GenerateDetailSettlementPaymentExcel(dataObjects.Result, models.Lang, "");
                    var file = new FileHelper().ReturnFormFile(dataObjects.Result.InfoSettlement.SettlementNo, stream, "Settlement Form - eFMS");
                    var model = new FileUploadAttachTemplateModel
                    {
                        Child = null,
                        File = file,
                        FolderName = "Settlement",
                        ModuleName = "Accounting",
                        Id = models.SettlementId,
                        UserCreated=dataObjects.Result.UserCreated
                    };
                    await _busControl.SendAsync(RabbitExchange.EFMS_FileManagement, RabbitConstants.PostAttachFileTemplateToEDocQueue, model);
                }, batchSize: 3, maxMessagesInFlight: 10);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            try
            {
                new Common.Helpers.LogHelper("GenFileSyncSMBackgroundService", "STOPPED at " + DateTime.Now);
                await base.StopAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                new Common.Helpers.LogHelper("GenFileSyncSMBackgroundService", " ERROR at " + DateTime.Now + " " + ex.ToString() + " ");
                throw;
            }

        }
    }
}
