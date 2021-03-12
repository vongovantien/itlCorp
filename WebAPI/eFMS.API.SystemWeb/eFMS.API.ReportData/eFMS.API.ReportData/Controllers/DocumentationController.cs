using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.ReportData.Consts;
using eFMS.API.ReportData.FormatExcel;
using eFMS.API.ReportData.Helpers;
using eFMS.API.ReportData.HttpServices;
using eFMS.API.ReportData.Models;
using eFMS.API.ReportData.Models.Criteria;
using eFMS.API.ReportData.Models.Documentation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace eFMS.API.ReportData.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    [ApiController]
    public class DocumentationController : Controller
    {
        private readonly APIs aPis;

        /// <summary>
        /// Contructor controller Accounting Report
        /// </summary>
        /// <param name="appSettings"></param>
        public DocumentationController(IOptions<APIs> appSettings)
        {
            this.aPis = appSettings.Value;
        }

        /// <summary>
        /// Export E-Manifest of a housebill
        /// </summary>
        /// <param name="hblid"></param>
        /// <returns></returns>
        [Route("ExportEManifest")]
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ExportEManifest(Guid hblid)
        {

            var accessToken = Request.Headers["Authorization"].ToString();
            var responseFromApi = await HttpServiceExtension.GetApi(aPis.HostStaging + Urls.Documentation.HouseBillDetailUrl + hblid, accessToken);

            var dataObject = responseFromApi.Content.ReadAsAsync<CsTransactionDetailModel>();
            var stream = new DocumentationHelper().CreateEManifestExcelFile(dataObject.Result);
            if (stream == null)
            {
                return null;
            }
            FileContentResult fileContent = new FileHelper().ExportExcel(stream, "E-Manifest.xlsx");

            return fileContent;
        }

        /// <summary>
        /// Export goods declare by house bill id
        /// </summary>
        /// <param name="hblid"></param>
        /// <returns></returns>
        [Route("ExportGoodsDeclare")]
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ExportGoodsDeclare(string hblid)
        {
            try
            {
                var accessToken = Request.Headers["Authorization"].ToString();
                var responseFromApi = await HttpServiceExtension.GetApi(aPis.HostStaging + Urls.Documentation.HouseBillDetailUrl + hblid, accessToken);

                var dataObject = responseFromApi.Content.ReadAsAsync<CsTransactionDetailModel>();

                var stream = new DocumentationHelper().CreateGoodsDeclare(dataObject.Result);
                if (stream == null)
                {
                    return null;
                }
                FileContentResult fileContent = new FileHelper().ExportExcel(stream, "Import Goods Declare.xlsx");

                return fileContent;
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// export dangerous goods
        /// </summary>
        /// <param name="hblid"></param>
        /// <returns></returns>
        [Route("ExportDangerousGoods")]
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ExportDangerousGoods(string hblid)
        {
            var accessToken = Request.Headers["Authorization"].ToString();
            var responseFromApi = await HttpServiceExtension.GetApi(aPis.HostStaging + Urls.Documentation.HouseBillDetailUrl + hblid, accessToken);

            var dataObject = responseFromApi.Content.ReadAsAsync<CsTransactionDetailModel>();

            var stream = new DocumentationHelper().CreateDangerousGoods(dataObject.Result);
            if (stream == null)
            {
                return null;
            }
            FileContentResult fileContent = new FileHelper().ExportExcel(stream, "Dangerous Goods.xlsx");

            return fileContent;
        }

        /// <summary>
        /// Export MAWB Air Export
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        [Route("ExportMAWBAirExport")]
        [HttpGet]
        public async Task<IActionResult> ExportMAWBAirExport(string jobId)
        {
            var responseFromApi = await HttpServiceExtension.GetApi(aPis.HostStaging + Urls.Documentation.AirwayBillExportUrl + jobId);

            var dataObject = responseFromApi.Content.ReadAsAsync<AirwayBillExportResult>();
            if (dataObject.Result == null)
            {
                return new FileHelper().ExportExcel(new MemoryStream(), "");
            }

            var stream = new DocumentationHelper().GenerateMAWBAirExportExcel(dataObject.Result);
            if (stream == null)
            {
                return new FileHelper().ExportExcel(new MemoryStream(), "");
            }
            FileContentResult fileContent = new FileHelper().ExportExcel(stream, "Air Export - MAWB.xlsx");
            
            return fileContent;
        }

        /// <summary>
        /// Export HAWB Air Export
        /// </summary>
        /// <param name="hblid"></param>
        /// <param name="officeId"></param>
        /// <returns></returns>
        [Route("ExportHAWBAirExport")]
        [HttpGet]
        public async Task<IActionResult> ExportHAWBAirExport(string hblid, string officeId)
        {
            var responseFromApi = await HttpServiceExtension.GetApi(aPis.HostStaging + Urls.Documentation.NeutralHawbExportUrl + "?housebillId=" + hblid + "&officeId=" + officeId);

            var dataObject = responseFromApi.Content.ReadAsAsync<AirwayBillExportResult>();
            if (dataObject.Result == null)
            {
                return new FileHelper().ExportExcel(new MemoryStream(), "");
            }

            var stream = new DocumentationHelper().GenerateHAWBAirExportExcel(dataObject.Result);
            if (stream == null)
            {
                return new FileHelper().ExportExcel(new MemoryStream(), "");
            }
            FileContentResult fileContent = new FileHelper().ExportExcel(stream, "Air Export - NEUTRAL HAWB.xlsx");

            return fileContent;
        }
        
        /// <summary>
        /// Export SCSC of MAWB Air Export
        /// </summary>
        /// <param name="jobId">Id of shipment</param>
        /// <returns></returns>
        [Route("ExportSCSCAirExport")]
        [HttpGet]
        public async Task<IActionResult> ExportSCSCAirExport(string jobId)
        {
            var responseFromApi = await HttpServiceExtension.GetApi(aPis.HostStaging + Urls.Documentation.AirwayBillExportUrl + jobId);

            var dataObject = responseFromApi.Content.ReadAsAsync<AirwayBillExportResult>();
            if (dataObject.Result == null)
            {
                return new FileHelper().ExportExcel(new MemoryStream(), "");
            }

            var stream = new DocumentationHelper().GenerateSCSCAirExportExcel(dataObject.Result);
            if (stream == null)
            {
                return new FileHelper().ExportExcel(new MemoryStream(), "");
            }
            FileContentResult fileContent = new FileHelper().ExportExcel(stream, "Air Export - Phiếu Cân SCSC.xlsx");

            return fileContent;
        }

        /// <summary>
        /// Export TCS of MAWB Air Export
        /// </summary>
        /// <param name="jobId">Id of shipment</param>
        /// <returns></returns>
        [Route("ExportTCSAirExport")]
        [HttpGet]
        public async Task<IActionResult> ExportTCSAirExport(string jobId)
        {
            var responseFromApi = await HttpServiceExtension.GetApi(aPis.HostStaging + Urls.Documentation.AirwayBillExportUrl + jobId);

            var dataObject = responseFromApi.Content.ReadAsAsync<AirwayBillExportResult>();
            if (dataObject.Result == null)
            {
                return new FileHelper().ExportExcel(new MemoryStream(), "");
            }

            var stream = new DocumentationHelper().GenerateTCSAirExportExcel(dataObject.Result);
            if (stream == null)
            {
                return new FileHelper().ExportExcel(new MemoryStream(), "");
            }
            FileContentResult fileContent = new FileHelper().ExportExcel(stream, "Air Export - Phiếu Cân TCS.xlsx");

            return fileContent;
        }

        /// <summary>
        /// Export ACS Excel For MAWB
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        [Route("ExportACSAirExport")]
        [HttpGet]
        public async Task<IActionResult> ExportACSAirExport(string jobId)
        {
            var responseFromApi = await HttpServiceExtension.GetApi(aPis.HostStaging + Urls.Documentation.AirwayBillExportUrl + jobId);

            var dataObject = responseFromApi.Content.ReadAsAsync<AirwayBillExportResult>();
            if (dataObject.Result == null)
            {
                return new FileHelper().ExportExcel(new MemoryStream(), "");
            }

            var stream = new DocumentationHelper().GenerateACSAirExportExcel(dataObject.Result, "PHIEU-CAN-ACS-Template.xlsx");
            if (stream == null)
            {
                return new FileHelper().ExportExcel(new MemoryStream(), "");
            }
            FileContentResult fileContent = new FileHelper().ExportExcel(stream, "Air Export - Phiếu Cân ACS.xlsx");

            return fileContent;
        }

        /// <summary>
        /// Export NCTS-ALS Excel For MAWB
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        [Route("ExportNCTSALSAirExport")]
        [HttpGet]
        public async Task<IActionResult> ExportNCTSALSAirExport(string jobId)
        {
            var responseFromApi = await HttpServiceExtension.GetApi(aPis.HostStaging + Urls.Documentation.AirwayBillExportUrl + jobId);

            var dataObject = responseFromApi.Content.ReadAsAsync<AirwayBillExportResult>();
            if (dataObject.Result == null)
            {
                return new FileHelper().ExportExcel(new MemoryStream(), "");
            }

            var stream = new DocumentationHelper().GenerateNCTSALSAirExportExcel(dataObject.Result, "PHIEU-CAN-NCTS-ALS-Template.xlsx");
            if (stream == null)
            {
                return new FileHelper().ExportExcel(new MemoryStream(), "");
            }
            FileContentResult fileContent = new FileHelper().ExportExcel(stream, "Air Export - Phiếu Cân NCTS & ALS.xlsx");

            return fileContent;
        }

        /// <summary>
        /// Export Shipment Overview
        /// </summary>
        /// <param name="criteria">Id of shipment</param>
        /// <returns></returns>
        [Route("ExportShipmentOverview")]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ExportShipmentOverview(GeneralReportCriteria criteria )
        {
            var accessToken = Request.Headers["Authorization"].ToString();
            var responseFromApi = await HttpServiceExtension.GetDataFromApi(criteria, aPis.HostStaging + Urls.Documentation.GetDataShipmentOverviewUrl);

            #region -- Ghi Log Report --
            var reportLogModel = new SysReportLogModel
            {
                ReportName = ResourceConsts.Shipment_Overview,
                ObjectParameter = JsonConvert.SerializeObject(criteria),
                Type = ResourceConsts.Export_Excel
            };
            var responseFromAddReportLog = await HttpServiceExtension.PostAPI(reportLogModel, aPis.HostStaging + Urls.Documentation.AddReportLogUrl, accessToken);
            #endregion -- Ghi Log Report --

            var dataObjects = responseFromApi.Content.ReadAsAsync<List<ExportShipmentOverview>>();
            if (dataObjects.Result == null)
            {
                return new FileHelper().ExportExcel(new MemoryStream(), "");
            }

            var stream = new DocumentationHelper().GenerateShipmentOverviewExcel(dataObjects.Result, criteria, null);
            if (stream == null)
            {
                return new FileHelper().ExportExcel(new MemoryStream(), "");
            }
            FileContentResult fileContent = new FileHelper().ExportExcel(stream, "Shipment Overview.xlsx");

            return fileContent;
        }

        /// <summary>
        /// Export Standard General Report
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [Route("ExportStandardGeneralReport")]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ExportStandardGeneralReport(GeneralReportCriteria criteria)
        {
            var accessToken = Request.Headers["Authorization"].ToString();
            var responseFromApi = await HttpServiceExtension.GetDataFromApi(criteria, aPis.HostStaging + Urls.Documentation.GetDataStandardGeneralReportUrl);

            #region -- Ghi Log Report --
            var reportLogModel = new SysReportLogModel
            {
                ReportName = ResourceConsts.Standard_Report,
                ObjectParameter = JsonConvert.SerializeObject(criteria),
                Type = ResourceConsts.Export_Excel
            };
            var responseFromAddReportLog = await HttpServiceExtension.PostAPI(reportLogModel, aPis.HostStaging + Urls.Documentation.AddReportLogUrl, accessToken);
            #endregion -- Ghi Log Report --

            var dataObjects = responseFromApi.Content.ReadAsAsync<List<GeneralReportResult>>();
            if (dataObjects.Result == null)
            {
                return new FileHelper().ExportExcel(new MemoryStream(), "");
            }

            var stream = new DocumentationHelper().GenerateStandardGeneralReportExcel(dataObjects.Result, criteria, null);
            if (stream == null)
            {
                return new FileHelper().ExportExcel(new MemoryStream(), "");
            }
            FileContentResult fileContent = new FileHelper().ExportExcel(stream, "Standard Report.xlsx");

            return fileContent;
        }

        /// <summary>
        /// Export Accounting PL Sheet
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [Route("ExportAccountingPlSheet")]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ExportAccountingPlSheet(GeneralReportCriteria criteria)
        {
            var accessToken = Request.Headers["Authorization"].ToString();
            var responseFromApi = await HttpServiceExtension.GetDataFromApi(criteria, aPis.HostStaging + Urls.Documentation.GetDataAccountingPLSheetUrl, accessToken);

            #region -- Ghi Log Report --
            var reportLogModel = new SysReportLogModel
            {
                ReportName = ResourceConsts.Accountant_PL_Sheet,
                ObjectParameter = JsonConvert.SerializeObject(criteria),
                Type = ResourceConsts.Export_Excel
            };
            var responseFromAddReportLog = await HttpServiceExtension.PostAPI(reportLogModel, aPis.HostStaging + Urls.Documentation.AddReportLogUrl, accessToken);
            #endregion -- Ghi Log Report --

            var dataObjects = responseFromApi.Content.ReadAsAsync<List<AccountingPlSheetExport>>();
            if (dataObjects.Result == null)
            {
                return new FileHelper().ExportExcel(new MemoryStream(), "");
            }

            var stream = new DocumentationHelper().GenerateAccountingPLSheetExcel(dataObjects.Result, criteria, null);
            if (stream == null)
            {
                return new FileHelper().ExportExcel(new MemoryStream(), "");
            }
            FileContentResult fileContent = new FileHelper().ExportExcel(stream, "Accounting PL Sheet.xlsx");

            return fileContent;
        }

        /// <summary>
        /// Export Job Profit Analysis
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [Route("ExportJobProfitAnalysis")]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ExportJobProfitAnalysis(GeneralReportCriteria criteria)
        {
            var accessToken = Request.Headers["Authorization"].ToString();
            var responseFromApi = await HttpServiceExtension.GetDataFromApi(criteria, aPis.HostStaging + Urls.Documentation.GetDataJobProfitAnalysisUrl, accessToken);
            var dataObjects = responseFromApi.Content.ReadAsAsync<List<JobProfitAnalysisExport>>();

            #region -- Ghi Log Report --
            var reportLogModel = new SysReportLogModel
            {
                ReportName = ResourceConsts.Job_Profit_Analysis,
                ObjectParameter = JsonConvert.SerializeObject(criteria),
                Type = ResourceConsts.Export_Excel
            };
            var responseFromAddReportLog = await HttpServiceExtension.PostAPI(reportLogModel, aPis.HostStaging + Urls.Documentation.AddReportLogUrl, accessToken);
            #endregion -- Ghi Log Report --

            if (dataObjects.Result == null)
            {
                return new FileHelper().ExportExcel(new MemoryStream(), "");
            }
            var stream = new DocumentationHelper().GenerateJobProfitAnalysisExportExcel(dataObjects.Result, criteria, null);
            if (stream == null)
            {
                return new FileHelper().ExportExcel(new MemoryStream(), "");
            }
            FileContentResult fileContent = new FileHelper().ExportExcel(stream, "Job Profit Analysis.xlsx");

            return fileContent;
        }
        
        /// Export Shipment Overview
        /// </summary>
        /// <param name="criteria">Id of shipment</param>
        /// <returns></returns>
        [Route("ExportSummaryOfCostsIncurred")]
        [HttpPost]
        public async Task<IActionResult> ExportSummaryOfCostsIncurred(GeneralReportCriteria criteria)
        {
            var accessToken = Request.Headers["Authorization"].ToString();
            var responseFromApi = await HttpServiceExtension.GetDataFromApi(criteria, aPis.HostStaging + Urls.Documentation.GetDataSummaryOfCostsIncurredUrl, accessToken);

            #region -- Ghi Log Report --
            var reportLogModel = new SysReportLogModel
            {
                ReportName = ResourceConsts.Summary_Of_Costs_Incurred,
                ObjectParameter = JsonConvert.SerializeObject(criteria),
                Type = ResourceConsts.Export_Excel
            };
            var responseFromAddReportLog = await HttpServiceExtension.PostAPI(reportLogModel, aPis.HostStaging + Urls.Documentation.AddReportLogUrl, accessToken);
            #endregion -- Ghi Log Report --

            var dataObjects = responseFromApi.Content.ReadAsAsync<List<SummaryOfCostsIncurredModel>>();
            if (dataObjects.Result == null || !dataObjects.Result.Any())
            {
                return new FileHelper().ExportExcel(new MemoryStream(), "");
            }

            var stream = new DocumentationHelper().GenerateSummaryOfCostsIncurredExcel(dataObjects.Result, criteria, null);
            if (stream == null)
            {
                return new FileHelper().ExportExcel(new MemoryStream(), "");
            }
            FileContentResult fileContent = new FileHelper().ExportExcel(stream, "Summary of Cossts incurred.xlsx");

            return fileContent;
        }

        /// <summary>
        /// Export Shipment Overview
        /// </summary>
        /// <param name="criteria">Id of shipment</param>
        /// <returns></returns>
        [Route("ExportSummaryOfRevenueIncurred")]
        [HttpPost]
        public async Task<IActionResult> ExportSummaryOfRevenueIncurred(GeneralReportCriteria criteria)
        {
            var accessToken = Request.Headers["Authorization"].ToString();
            var responseFromApi = await HttpServiceExtension.GetDataFromApi(criteria, aPis.HostStaging + Urls.Documentation.GetDataSummaryOfRevenueIncurredUrl, accessToken);

            #region -- Ghi Log Report --
            var reportLogModel = new SysReportLogModel
            {
                ReportName = ResourceConsts.Summary_Of_Revenue_Incurred,
                ObjectParameter = JsonConvert.SerializeObject(criteria),
                Type = ResourceConsts.Export_Excel
            };
            var responseFromAddReportLog = await HttpServiceExtension.PostAPI(reportLogModel, aPis.HostStaging + Urls.Documentation.AddReportLogUrl, accessToken);
            #endregion -- Ghi Log Report --

            var dataObjects = responseFromApi.Content.ReadAsAsync<SummaryOfRevenueModel>();
            if (dataObjects.Result == null || !dataObjects.Result.summaryOfRevenueExportResults.Any())
            {
                return new FileHelper().ExportExcel(new MemoryStream(), "");
            }

            var stream = new DocumentationHelper().GenerateSummaryOfRevenueExcel(dataObjects.Result,criteria, null);
            if (stream == null)
            {
                return new FileHelper().ExportExcel(new MemoryStream(), "");
            }
            FileContentResult fileContent = new FileHelper().ExportExcel(stream, "Summary of Revenue incurred.xlsx");
            return fileContent;
        }

        /// <summary>
        /// Export Shipment Overview
        /// </summary>
        /// <param name="criteria">Id of shipment</param>
        /// <returns></returns>
        [Route("ExportSummaryOfCostsPartner")]
        [HttpPost]
        public async Task<IActionResult> ExportSummaryOfCostsPartner(GeneralReportCriteria criteria)
        {
            var accessToken = Request.Headers["Authorization"].ToString();
            var responseFromApi = await HttpServiceExtension.GetDataFromApi(criteria, aPis.HostStaging + Urls.Documentation.GetDataSummaryOfCostsPartnerUrl, accessToken);

            #region -- Ghi Log Report --
            var reportLogModel = new SysReportLogModel
            {
                ReportName = ResourceConsts.Summary_Of_Revenue_Incurred,
                ObjectParameter = JsonConvert.SerializeObject(criteria),
                Type = ResourceConsts.Export_Excel
            };
            var responseFromAddReportLog = await HttpServiceExtension.PostAPI(reportLogModel, aPis.HostStaging + Urls.Documentation.AddReportLogUrl, accessToken);
            #endregion -- Ghi Log Report --

            var dataObjects = responseFromApi.Content.ReadAsAsync<SummaryOfRevenueModel>();
            if (dataObjects.Result == null || !dataObjects.Result.summaryOfRevenueExportResults.Any())
            {
                return new FileHelper().ExportExcel(new MemoryStream(), "");
            }

            var stream = new DocumentationHelper().GenerateSummaryOfRevenueExcel(dataObjects.Result, criteria, null);
            if (stream == null)
            {
                return new FileHelper().ExportExcel(new MemoryStream(), "");
            }
            FileContentResult fileContent = new FileHelper().ExportExcel(stream, "Costs By Partner.xlsx");
            return fileContent;
        }

        [Route("ExportHousebillDaily")]
        [HttpGet]
        public async Task<IActionResult> ExportHousebillDaily(DateTime? issuedDate)
        {
            if (issuedDate == null)
            {
                return new FileHelper().ExportExcel(new MemoryStream(), "");
            }
            var responseFromApi = await HttpServiceExtension.GetApi(aPis.HostStaging + Urls.Documentation.GetDataHousebillDailyExportUrl + issuedDate.Value.ToString("yyyy-MM-dd"));

            var dataObject = responseFromApi.Content.ReadAsAsync<List<HousebillDailyExportResult>>();
            if (dataObject.Result == null || dataObject.Result.Count == 0)
            {
                return new FileHelper().ExportExcel(new MemoryStream(), "");
            }

            var stream = new DocumentationHelper().GenerateHousebillDailyExportExcel(dataObject.Result, issuedDate);
            if (stream == null)
            {
                return new FileHelper().ExportExcel(new MemoryStream(), "");
            }
            FileContentResult fileContent = new FileHelper().ExportExcel(stream, "DAILY LIST "+ issuedDate.Value.ToString("dd MMM yyyy").ToUpper() + ".xlsx");

            return fileContent;
        }

        /// <summary>
        /// Export Ops Debit Note
        /// </summary>
        /// <returns></returns>
        [Route("ExportOpsCdNote")]
        [HttpGet]
        public async Task<IActionResult> ExportOpsCdNote(Guid jobId, string cdNo, Guid officeId)
        {
            var responseFromApi = await HttpServiceExtension.GetApi(aPis.HostStaging + Urls.Documentation.GetDataCDNoteExportUrl + jobId + "&cdNo=" + cdNo + "&officeId=" + officeId);

            var dataObjects = responseFromApi.Content.ReadAsAsync<AcctCDNoteExportResult>();
            if (dataObjects.Result == null)
            {
                return Ok();
            }
            var stream = new DocumentationHelper().GenerateCDNoteDetailExcel(dataObjects.Result);
            if (stream == null)
            {
                return null;
            }
            string fileName = "Export OPS CD Note.xlsx";
            FileContentResult fileContent = new FileHelper().ExportExcel(stream, fileName);
            return fileContent;
        }

        /// <summary>
        /// Export Commission PR For OPS Report
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="currentUserId"></param>
        /// <param name="rptType"></param>
        /// <returns></returns>
        [Route("ExportCommissionPRReport")]
        [HttpPost]
        public async Task<IActionResult> ExportCommissionPRReport(CommissionReportCriteria criteria, string currentUserId, string rptType)
        {
            var responseFromApi = await HttpServiceExtension.GetDataFromApi(criteria, aPis.HostStaging + Urls.Documentation.GetDataCommissionPRReportUrl + currentUserId + "&rptType=" + rptType);

            var dataObjects = responseFromApi.Content.ReadAsAsync<CommissionExportResult>();
            if (dataObjects.Result == null)
            {
                return new FileHelper().ExportExcel(new MemoryStream(), "");
            }
            Stream stream;
            if (rptType == "OPS")
            {
                stream = new DocumentationHelper().GenerateCommissionPROpsReportExcel(dataObjects.Result, criteria, null);
            }
            else
            {
                stream = new DocumentationHelper().GenerateCommissionPRReportExcel(dataObjects.Result, criteria, null);
            }

            if (stream == null)
            {
                return new FileHelper().ExportExcel(new MemoryStream(), "");
            }
            FileContentResult fileContent;
            if(rptType == "OPS")
            {
                fileContent = new FileHelper().ExportExcel(stream, "Commission OPS VND.xlsx");
            }
            else
            {
                fileContent = new FileHelper().ExportExcel(stream, "Commission PR.xlsx");
            }
            return fileContent;
        }

        /// <summary>
        /// Export Incentive Report
        /// </summary>
        /// <returns></returns>
        [Route("ExportIncentiveReport")]
        [HttpPost]
        public async Task<IActionResult> ExportIncentiveReport(CommissionReportCriteria criteria, string currentUserId)
        {
            var responseFromApi = await HttpServiceExtension.GetDataFromApi(criteria, aPis.HostStaging + Urls.Documentation.GetDataIncentiveReportUrl + currentUserId);

            var dataObjects = responseFromApi.Content.ReadAsAsync<CommissionExportResult>();
            if (dataObjects.Result == null)
            {
                return new FileHelper().ExportExcel(new MemoryStream(), "");
            }
            var stream = new DocumentationHelper().GenerateIncentiveReportExcel(dataObjects.Result, criteria, null);
            if (stream == null)
            {
                return new FileHelper().ExportExcel(new MemoryStream(), "");
            }
            string fileName = "Incentive.xlsx";
            FileContentResult fileContent = new FileHelper().ExportExcel(stream, fileName);
            return fileContent;
        }
        
        /// <summary>
        /// Export Combine Ops Debit Note
        /// </summary>
        /// <returns></returns>
        [Route("ExportOpsCdNoteCombine")]
        [HttpPost]
        public async Task<IActionResult> ExportOpsCdNoteCombine(object model)
        {
            var responseFromApi = await HttpServiceExtension.GetDataFromApi(model, aPis.HostStaging + Urls.Documentation.GetDataCDNoteCombineExportUrl);

            var dataObjects = responseFromApi.Content.ReadAsAsync<AcctCDNoteExportResult>();
            if (dataObjects.Result == null)
            {
                return Ok();
            }
            var stream = new DocumentationHelper().GenerateCDNoteDetailExcel(dataObjects.Result);
            if (stream == null)
            {
                return null;
            }
            string fileName = "Export OPS CD Note.xlsx";
            FileContentResult fileContent = new FileHelper().ExportExcel(stream, fileName);
            return fileContent;
        }
    }
}
