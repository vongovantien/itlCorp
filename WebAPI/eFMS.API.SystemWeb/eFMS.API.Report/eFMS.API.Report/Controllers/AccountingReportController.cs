using eFMS.API.Report.DL.IService;
using eFMS.API.Report.DL.Models;
using eFMS.API.Report.Infrastructure.Middlewares;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    }
}