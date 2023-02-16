using eFMS.API.Common.Helpers;
using eFMS.API.Report.DL.Common;
using eFMS.API.Report.DL.IService;
using eFMS.API.Report.DL.Models;
using eFMS.API.Report.Helpers;
using eFMS.API.Report.Infrastructure.Middlewares;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.Report.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class AccountingReportController : Controller
    {
        private IAccountingReportService accountingReport;
        public AccountingReportController(IAccountingReportService accountingR)
        {
            accountingReport = accountingR;
        }

        [HttpPost("GetDataExportAccountingPlSheet")]
        [Authorize]
        public IActionResult GetDataExportAccountingPlSheet(GeneralReportCriteria criteria)
        {
            var data = accountingReport.GetDataAccountingPLSheet(criteria);
            return Ok(data);
        }
        

        [HttpPost("GetDataSummaryOfCostsIncurred")]
        [Authorize]
        public IActionResult GetDataSummaryOfCostsIncurred(GeneralReportCriteria criteria)
        {
            var data = accountingReport.GetDataSummaryOfCostsIncurred(criteria);
            return Ok(data);
        }

        [HttpPost("GetDataSummaryOfRevenueIncurred")]
        [Authorize]
        public IActionResult GetDataSummaryOfRevenueIncurred(GeneralReportCriteria criteria)
        {
            var data = accountingReport.GetDataSummaryOfRevenueIncurred(criteria);
            return Ok(data);
        }

        [HttpPost("GetDataCostsByPartner")]
        [Authorize]
        public IActionResult GetDataCostsByPartner(GeneralReportCriteria criteria)
        {
            var data = accountingReport.GetDataCostsByPartner(criteria);
            return Ok(data);
        }

        [HttpPost("GetDataJobProfitAnalysis")]
        [Authorize]
        public IActionResult GetDataJobProfitAnalysis(GeneralReportCriteria criteria)
        {
            var data = accountingReport.GetDataJobProfitAnalysis(criteria);
            return Ok(data);
        }

        [Route("ExportAccountingPlSheet")]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ExportAccountingPlSheet(GeneralReportCriteria criteria)
        {
            #region -- Ghi Log Report --
            var reportLogModel = new SysReportLogModel
            {
                ReportName = ReportConstants.Accountant_PL_Sheet,
                ObjectParameter = JsonConvert.SerializeObject(criteria),
                Type = ReportConstants.Export_Excel
            };
            #endregion -- Ghi Log Report --
            var data = accountingReport.GetDataAccountingPLSheet(criteria);
            if (data == null)
            {
                return Ok(null);
                //return new Helpers.FileHelper().ExportExcel(null, new MemoryStream(), "");
            }
            new LogHelper("ExportAccountingPlSheet", "" + data.Count().ToString());
            //var stream = new ReportHelper().GenerateAccountingPLSheetExcel(data, criteria, null);
            var stream = new ReportHelper().BindingDataAccountingPLSheetExportExcel(data, criteria);
            if (stream == null)
            {
                new LogHelper("Stream null");
                return new Helpers.FileHelper().ExportExcel(null, new MemoryStream(), "");
            }
            new LogHelper("Stream not null");
            FileContentResult fileContent = new Helpers.FileHelper().ExportExcel(null, stream, "Accounting PL Sheet" + criteria.Currency);
            HeaderResponse(fileContent.FileDownloadName);
            return fileContent;
        }        

        [Route("ExportJobProfitAnalysis")]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ExportJobProfitAnalysis(GeneralReportCriteria criteria)
        {
            #region -- Ghi Log Report --
            var reportLogModel = new SysReportLogModel
            {
                ReportName = ReportConstants.Job_Profit_Analysis,
                ObjectParameter = JsonConvert.SerializeObject(criteria),
                Type = ReportConstants.Export_Excel
            };
            #endregion -- Ghi Log Report --

            var data = accountingReport.GetDataJobProfitAnalysis(criteria);
            if (data == null)
            {
                return new Helpers.FileHelper().ExportExcel(null, new MemoryStream(), "");
            }

            var stream = new ReportHelper().GenerateJobProfitAnalysisExportExcel(data, criteria, null);
            if (stream == null)
            {
                return new Helpers.FileHelper().ExportExcel(null, new MemoryStream(), "");
            }
            FileContentResult fileContent = new Helpers.FileHelper().ExportExcel(null, stream, "Job Profit Analysis");
            HeaderResponse(fileContent.FileDownloadName);
            return fileContent;
        }

        [Route("ExportSummaryOfCostsIncurred")]
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ExportSummaryOfCostsIncurred(GeneralReportCriteria criteria)
        {

            #region -- Ghi Log Report --
            var reportLogModel = new SysReportLogModel
            {
                ReportName = ReportConstants.Summary_Of_Costs_Incurred,
                ObjectParameter = JsonConvert.SerializeObject(criteria),
                Type = ReportConstants.Export_Excel
            };
            #endregion -- Ghi Log Report --
            var data = accountingReport.GetDataSummaryOfCostsIncurred(criteria);
            if (data == null)
            {
                return new Helpers.FileHelper().ExportExcel(null, new MemoryStream(), "");
            }

            var stream = new ReportHelper().GenerateSummaryOfCostsIncurredExcel(data, criteria, null);
            if (stream == null)
            {
                return new Helpers.FileHelper().ExportExcel(null, new MemoryStream(), "");
            }
            FileContentResult fileContent = new Helpers.FileHelper().ExportExcel(null, stream, "Summary of Cossts incurred");
            HeaderResponse(fileContent.FileDownloadName);
            return fileContent;
        }

        [Route("ExportSummaryOfRevenueIncurred")]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ExportSummaryOfRevenueIncurred(GeneralReportCriteria criteria)
        {
            #region -- Ghi Log Report --
            var reportLogModel = new SysReportLogModel
            {
                ReportName = ReportConstants.Summary_Of_Revenue_Incurred,
                ObjectParameter = JsonConvert.SerializeObject(criteria),
                Type = ReportConstants.Export_Excel
            };
            #endregion -- Ghi Log Report --

            var data = accountingReport.GetDataSummaryOfRevenueIncurred(criteria);
            if (data == null)
            {
                return new Helpers.FileHelper().ExportExcel(null, new MemoryStream(), "");
            }

            var stream = new ReportHelper().GenerateSummaryOfRevenueExcel(data, criteria, null);
            if (stream == null)
            {
                return new Helpers.FileHelper().ExportExcel(null, new MemoryStream(), "");
            }
            FileContentResult fileContent = new Helpers.FileHelper().ExportExcel(null, stream, "Summary of Revenue incurred");
            HeaderResponse(fileContent.FileDownloadName);
            return fileContent;
        }

        [Route("ExportSummaryOfCostsPartner")]
        [HttpPost]
        public async Task<IActionResult> ExportSummaryOfCostsPartner(GeneralReportCriteria criteria)
        {
            #region -- Ghi Log Report --
            var reportLogModel = new SysReportLogModel
            {
                ReportName = ReportConstants.Summary_Of_Revenue_Incurred,
                ObjectParameter = JsonConvert.SerializeObject(criteria),
                Type = ReportConstants.Export_Excel
            };
            #endregion -- Ghi Log Report --

            var data = accountingReport.GetDataCostsByPartner(criteria);
            if (data == null)
            {
                return new Helpers.FileHelper().ExportExcel(null, new MemoryStream(), "");
            }

            var stream = new ReportHelper().GenerateSummaryOfRevenueExcel(data, criteria, null);

            if (stream == null)
            {
                return new Helpers.FileHelper().ExportExcel(null, new MemoryStream(), "");
            }
            FileContentResult fileContent = new Helpers.FileHelper().ExportExcel(null, stream, "Costs By Partner");
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