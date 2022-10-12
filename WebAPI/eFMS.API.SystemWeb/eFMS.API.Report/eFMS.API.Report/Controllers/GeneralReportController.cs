using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Report.DL.IService;
using eFMS.API.Report.DL.Models;
using eFMS.API.Report.Infrastructure.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace eFMS.API.Report.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class GeneralReportController : ControllerBase
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

        [HttpPost("GetDataExportShipmentOverview")]
        public IActionResult GetDataExportShipmentOverview(GeneralReportCriteria criteria)
        {
            var data = generalReportService.GetDataGeneralExportShipmentOverview(criteria);
            var result = data;
            return Ok(result);
        }
    }
}