using System.IO;
using System.Threading.Tasks;
using eFMS.API.Common.Helpers;
using eFMS.API.Report.DL.Common;
using eFMS.API.Report.DL.IService;
using eFMS.API.Report.DL.Models;
using eFMS.API.Report.Helpers;
using eFMS.API.Report.Infrastructure.Middlewares;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace eFMS.API.Report.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class GeneralReportController : Controller
    {
        private IGeneralReportService generalReportService;
        public GeneralReportController(IGeneralReportService generalReport)
        {
            generalReportService = generalReport;
        }

        [HttpPost("GetDataGeneralReport")]
        public IActionResult GetDataGeneralReport(GeneralReportCriteria criteria, int page, int size)
        {
            var data = generalReportService.GetDataGeneralReport(criteria, page, size, out int rowCount);
            var result = new { data, totalItems = rowCount, page, size };
            return Ok(result);
        }

        [HttpPost("QueryDataGeneralReport")]
        public IActionResult QueryDataGeneralReport(GeneralReportCriteria criteria)
        {
            var data = generalReportService.QueryDataGeneralReport(criteria);
            return Ok(data);
        }

        [HttpPost("QueryDataEDocReport")]
        public IActionResult QueryDataEDocReport(GeneralReportCriteria criteria)
        {
            var data = generalReportService.QueryDataEDocsReport(criteria);
            return Ok(data);
        }

        [HttpPost("GetDataExportShipmentOverview")]
        public IActionResult GetDataExportShipmentOverview(GeneralReportCriteria criteria)
        {
            var data = generalReportService.GetDataGeneralExportShipmentOverview(criteria);
            var result = data;
            return Ok(result);
        }

        [HttpPost("GetDataExportShipmentOverviewFCL")]
        public IActionResult GetDataExportShipmentOverviewFCL(GeneralReportCriteria criteria)
        {
            var data = generalReportService.GetDataGeneralExportShipmentOverviewFCL(criteria);
            var result = data;
            return Ok(result);
        }

        [HttpPost("GetDataExportShipmentOverviewLCL")]
        public IActionResult GetDataExportShipmentOverviewLCL(GeneralReportCriteria criteria)
        {
            var data = generalReportService.GetDataGeneralExportShipmentOverviewLCL(criteria);
            var result = data;
            return Ok(result);
        }

        [Route("ExportShipmentOverview")]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ExportShipmentOverview(GeneralReportCriteria criteria)
        {
            #region -- Ghi Log Report --
            var reportLogModel = new SysReportLogModel
            {
                ReportName = ReportConstants.Shipment_Overview,
                ObjectParameter = JsonConvert.SerializeObject(criteria),
                Type = ReportConstants.Export_Excel
            };
            #endregion -- Ghi Log Report --

            var data = generalReportService.GetDataGeneralExportShipmentOverview(criteria);
            if (data == null)
            {
                return new Helpers.FileHelper().ExportExcel(null, new MemoryStream(), "");
            }

            var stream = new ReportHelper().GenerateShipmentOverviewExcel(data, criteria, null);
            if (stream == null)
            {
                return new Helpers.FileHelper().ExportExcel(null, new MemoryStream(), "");
            }
            FileContentResult fileContent = new Helpers.FileHelper().ExportExcel(null, stream, "Shipment Overview");
            HeaderResponse(fileContent.FileDownloadName);
            return fileContent;
        }

        [Route("ExportShipmentOverviewWithType")]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ExportShipmentOverviewWithType(GeneralReportCriteria criteria, string reportType)
        {
            #region -- Ghi Log Report --
            var reportLogModel = new SysReportLogModel
            {
                ReportName = ReportConstants.Shipment_Overview,
                ObjectParameter = JsonConvert.SerializeObject(criteria),
                Type = ReportConstants.Export_Excel
            };
            #endregion -- Ghi Log Report --
            
            Stream stream;
            if (reportType == "FCL")
            {
                var data = generalReportService.GetDataGeneralExportShipmentOverviewFCL(criteria);
                if (data == null)
                {
                    return new Helpers.FileHelper().ExportExcel(null, new MemoryStream(), "");
                }

                stream = new ReportHelper().GenerateShipmentOverviewFCLExcell(data, criteria);
            }
            else
            {
                var data = generalReportService.GetDataGeneralExportShipmentOverviewLCL(criteria);
                if (data == null)
                {
                    return new Helpers.FileHelper().ExportExcel(null, new MemoryStream(), "");
                }
                stream = new ReportHelper().BidingGeneralLCLExport(data, criteria, "ShipmentOverviewLCL");
            }

            if (stream == null)
            {
                return new Helpers.FileHelper().ExportExcel(null, new MemoryStream(), "");
            }

            var downloadName = reportType == "FCL" ? "Shipment Overview FCL" : "Shipment Overview-LCL";
            FileContentResult fileContent = new Helpers.FileHelper().ExportExcel(null, stream, downloadName);
            HeaderResponse(fileContent.FileDownloadName);
            return fileContent;
        }

        [Route("ExportStandardGeneralReport")]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ExportStandardGeneralReport(GeneralReportCriteria criteria)
        {
            #region -- Ghi Log Report --
            var reportLogModel = new SysReportLogModel
            {
                ReportName = ReportConstants.Standard_Report,
                ObjectParameter = JsonConvert.SerializeObject(criteria),
                Type = ReportConstants.Export_Excel
            };
            #endregion -- Ghi Log Report --

            var data = generalReportService.QueryDataGeneralReport(criteria);
            if (data == null)
            {
                return new Helpers.FileHelper().ExportExcel(null, new MemoryStream(), "");
            }

            var stream = new ReportHelper().GenerateStandardGeneralReportExcel(data, criteria, null);
            if (stream == null)
            {
                return new Helpers.FileHelper().ExportExcel(null, new MemoryStream(), "");
            }
            FileContentResult fileContent = new Helpers.FileHelper().ExportExcel(null, stream, "Standard Report" + criteria.Currency);
            HeaderResponse(fileContent.FileDownloadName);
            return fileContent;
        }

        [Route("ExportEDocTemplateReport")]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ExportEDocTemplateReport(GeneralReportCriteria criteria)
        {
            #region -- Ghi Log Report --
            var reportLogModel = new SysReportLogModel
            {
                ReportName = ReportConstants.Standard_Report,
                ObjectParameter = JsonConvert.SerializeObject(criteria),
                Type = ReportConstants.Export_Excel
            };
            #endregion -- Ghi Log Report --

            var data = generalReportService.QueryDataEDocsReport(criteria);
            if (data == null)
            {
                return new Helpers.FileHelper().ExportExcel(null, new MemoryStream(), "");
            }
            var stream = new ReportHelper().BindingDataEDocReport(data, criteria);
            if (stream == null)
            {
                return new Helpers.FileHelper().ExportExcel(null, new MemoryStream(), "");
            }
            FileContentResult fileContent = new Helpers.FileHelper().ExportExcel(null, stream, "EdocReportTemplate");
            HeaderResponse(fileContent.FileDownloadName);
            return fileContent;
        }



        private void HeaderResponse(string fileName)
        {
            Response.Headers.Add("efms-file-name", fileName);
            Response.Headers.Add("Access-Control-Expose-Headers", "efms-file-name");
        }
    }
}