
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
                new Common.Helpers.LogHelper("GenFileSyncSMBackgroundService", "RUNNING at " + DateTime.Now);
                await _busControl.ReceiveAsync<ExportDetailSettleModel>(RabbitExchange.EFMS_ReportData, RabbitConstants.GenFileQueue, async (models) =>
                {
                    Console.WriteLine("==================== GenFileSyncSMBackgroundService ============================");
                    new Common.Helpers.LogHelper("GenFileSyncSMBackgroundService", "EXCUTE at " + DateTime.Now + " " + JsonConvert.SerializeObject(models));
                    //using (var scope = _services.CreateScope())
                    //{
                    //    var scopedService = scope.ServiceProvider.GetRequiredService<IEDocService>();
                    //    var d = await scopedService.PostAttachFileTemplateToEDoc(models);
                    //}
                    var responseFromApi = await HttpServiceExtension.GetApi(aPis.AccountingAPI + Urls.Accounting.DetailSettlementPaymentExportUrl + "?settlementId=" + models.SettlementId, models.AccessToken);
                    var dataObjects = responseFromApi.Content.ReadAsAsync<SettlementExport>();
                    var stream = new AccountingHelper().GenerateDetailSettlementPaymentExcel(dataObjects.Result, models.Lang, "");
                    //if (stream == null) return new FileHelper().ExportExcel(null, new MemoryStream(), "");

                    var file = new FileHelper().ReturnFormFile(dataObjects.Result.InfoSettlement.SettlementNo, stream, "Settlement Form - eFMS");
                    if (models.Action == "Preview")
                    {
                        //string previewURL = Urls.Accounting.UploadFileExcel + ResourceConsts.FolderPreviewUploadFile;
                        var response = await HttpServiceExtension.PutDataToApi(file, aPis.FileManagementAPI + Urls.Accounting.UploadFileExcel + ResourceConsts.FolderPreviewUploadFile + "/" + models.SettlementId, models.AccessToken);
                    }
                    else
                    {
                        var model = new FileUploadAttachTemplateModel
                        {
                            Child=null,
                            File=file,
                            FolderName= "Settlement",
                            ModuleName="Accounting",
                            Id=models.SettlementId,
                        };
                        await _busControl.SendAsync(RabbitExchange.EFMS_FileManagement, RabbitConstants.PostAttachFileTemplateToEDocQueue, model);
                    }

                    Console.WriteLine("==================== GenFileSyncSMBackgroundService ============================");

                }, batchSize: 3, maxMessagesInFlight: 10);
            }
            catch (Exception ex)
            {
                new Common.Helpers.LogHelper("GenFileSyncSMBackgroundService", " ERROR at " + DateTime.Now + " " + ex.ToString() + " ");
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
