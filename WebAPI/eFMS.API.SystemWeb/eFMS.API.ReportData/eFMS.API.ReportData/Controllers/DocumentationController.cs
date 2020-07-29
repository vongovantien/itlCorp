using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.ReportData.FormatExcel;
using eFMS.API.ReportData.Helpers;
using eFMS.API.ReportData.HttpServices;
using eFMS.API.ReportData.Models;
using eFMS.API.ReportData.Models.Criteria;
using eFMS.API.ReportData.Models.Documentation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

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
        /// Export Shipment Overview
        /// </summary>
        /// <param name="criteria">Id of shipment</param>
        /// <returns></returns>
        [Route("ExportShipmentOverview")]
        [HttpPost]
        public async Task<IActionResult> ExportShipmentOverview(GeneralReportCriteria criteria )
        {
            var responseFromApi = await HttpServiceExtension.GetDataFromApi(criteria, aPis.HostStaging + Urls.Documentation.GetDataShipmentOverviewUrl);

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
        public async Task<IActionResult> ExportStandardGeneralReport(GeneralReportCriteria criteria)
        {
            var responseFromApi = await HttpServiceExtension.GetDataFromApi(criteria, aPis.HostStaging + Urls.Documentation.GetDataStandardGeneralReportUrl);

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
        /// Export Shipment Overview
        /// </summary>
        /// <param name="criteria">Id of shipment</param>
        /// <returns></returns>
        [Route("ExportSummaryOfCostsIncurred")]
        [HttpPost]
        public async Task<IActionResult> ExportSummaryOfCostsIncurred(GeneralReportCriteria criteria)
        {
            var responseFromApi = await HttpServiceExtension.GetDataFromApi(criteria, aPis.HostStaging + Urls.Documentation.GetDataSummaryOfCostsIncurredUrl);

            var dataObjects = responseFromApi.Content.ReadAsAsync<List<SummaryOfCostsIncurredModel>>();
            if (dataObjects.Result == null)
            {
                return new FileHelper().ExportExcel(new MemoryStream(), "");
            }

            var stream = new DocumentationHelper().GenerateSummaryOfCostsIncurredExcel(dataObjects.Result, null);
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
            var responseFromApi = await HttpServiceExtension.GetDataFromApi(criteria, aPis.HostStaging + Urls.Documentation.GetDataSummaryOfRevenueIncurredUrl);

            var dataObjects = responseFromApi.Content.ReadAsAsync<SummaryOfRevenueModel>();
            if (dataObjects.Result == null)
            {
                return new FileHelper().ExportExcel(new MemoryStream(), "");
            }

            var stream = new DocumentationHelper().GenerateSummaryOfRevenueExcel(dataObjects.Result, null);
            if (stream == null)
            {
                return new FileHelper().ExportExcel(new MemoryStream(), "");
            }
            FileContentResult fileContent = new FileHelper().ExportExcel(stream, "Summary of Revenue incurred.xlsx");
            return fileContent;
        }
    }
}
