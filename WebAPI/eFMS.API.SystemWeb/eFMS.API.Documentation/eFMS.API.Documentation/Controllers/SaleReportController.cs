using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.ReportResults.Sales;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
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
        readonly IReportLogService reportLogService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="service"></param>
        /// <param name="localizer"></param>
        /// <param name="rptLogService"></param>
        public SaleReportController(ISaleReportService service, IStringLocalizer<LanguageSub> localizer, IReportLogService rptLogService)
        {
            saleReportService = service;
            stringLocalizer = localizer;
            reportLogService = rptLogService;
        }

        [HttpPost]
        [Authorize]
        public IActionResult MonthlySalereport(SaleReportCriteria criteria)
        {
            var result = saleReportService.PreviewGetMonthlySaleReport(criteria);
            
            #region -- Ghi log Report --
            var reportLogModel = new SysReportLogModel
            {
                ReportName = DocumentConstants.Monthly_Sale_Report,
                ObjectParameter = JsonConvert.SerializeObject(criteria),
                Type = DocumentConstants.Crystal_Preview
            };
            reportLogService.AddNew(reportLogModel);
            #endregion -- Ghi log Report --

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
            
            #region -- Ghi log Report --
            var reportLogModel = new SysReportLogModel
            {
                ReportName = DocumentConstants.Sale_Report_By_Quater,
                ObjectParameter = JsonConvert.SerializeObject(criteria),
                Type = DocumentConstants.Crystal_Preview
            };
            reportLogService.AddNew(reportLogModel);
            #endregion -- Ghi log Report --

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
            
            #region -- Ghi log Report --
            var reportLogModel = new SysReportLogModel
            {
                ReportName = DocumentConstants.Sale_Report_By_Department,
                ObjectParameter = JsonConvert.SerializeObject(criteria),
                Type = DocumentConstants.Crystal_Preview
            };
            reportLogService.AddNew(reportLogModel);
            #endregion -- Ghi log Report --

            return Ok(result);
        }

        [HttpPost("SummarySaleReport")]
        [Authorize]
        public IActionResult SummarySaleReport(SaleReportCriteria criteria)
        {
            var result = saleReportService.PreviewSummarySaleReport(criteria);
            
            #region -- Ghi log Report --
            var reportLogModel = new SysReportLogModel
            {
                ReportName = DocumentConstants.Summary_Sale_Report,
                ObjectParameter = JsonConvert.SerializeObject(criteria),
                Type = DocumentConstants.Crystal_Preview
            };
            reportLogService.AddNew(reportLogModel);
            #endregion -- Ghi log Report --

            return Ok(result);
        }

        [HttpPost("CombinationSaleReport")]
        [Authorize]
        public IActionResult CombinationSaleReport(SaleReportCriteria criteria)
        {
            var result = saleReportService.PreviewCombinationSaleReport(criteria);
            
            #region -- Ghi log Report --
            var reportLogModel = new SysReportLogModel
            {
                ReportName = DocumentConstants.Combination_Statistic_Report,
                ObjectParameter = JsonConvert.SerializeObject(criteria),
                Type = DocumentConstants.Crystal_Preview
            };
            reportLogService.AddNew(reportLogModel);
            #endregion -- Ghi log Report --

            return Ok(result);
        }
    }
}
