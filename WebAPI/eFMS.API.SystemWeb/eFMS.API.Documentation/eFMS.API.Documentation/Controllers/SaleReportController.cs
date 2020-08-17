using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models.ReportResults.Sales;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using SystemManagementAPI.Infrastructure.Middlewares;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace eFMS.API.Documentation.Controllers
{
    /// <summary>
    /// A base class for an MVC controller without view support.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class SaleReportController : ControllerBase
    {
        readonly ISaleReportService saleReportService;
        readonly IStringLocalizer stringLocalizer;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="service"></param>
        /// <param name="localizer"></param>
        public SaleReportController(ISaleReportService service, IStringLocalizer<LanguageSub> localizer)
        {
            saleReportService = service;
            stringLocalizer = localizer;
        }

        [HttpPost]
        [Authorize]
        public IActionResult MonthlySalereport(SaleReportCriteria criteria)
        {
            var result = saleReportService.PreviewGetMonthlySaleReport(criteria);
            return Ok(result);
        }

        /// <summary>
        /// Get data sale report by quater
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPost("QuaterSaleReport")]
        [Authorize]
        public IActionResult QuaterSaleReport(SaleReportCriteria criteria)
        {
            var result = saleReportService.PreviewGetQuaterSaleReport(criteria);
            return Ok(result);
        }

        /// <summary>
        /// Get data sale report by department
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPost("DepartSaleReport")]
        [Authorize]
        public IActionResult DepartSaleReport(SaleReportCriteria criteria)
        {
            var result = saleReportService.PreviewGetDepartSaleReport(criteria);
            return Ok(result);
        }

        [HttpPost("SummarySaleReport")]
        [Authorize]
        public IActionResult SummarySaleReport(SaleReportCriteria criteria)
        {
            var result = saleReportService.PreviewSummarySaleReport(criteria);
            return Ok(result);
        }

        [HttpPost("CombinationSaleReport")]
        [Authorize]
        public IActionResult CombinationSaleReport(SaleReportCriteria criteria)
        {
            var result = saleReportService.PreviewCombinationSaleReport(criteria);
            return Ok(result);
        }
    }
}
