﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using eFMS.API.Report.DL.Common;
using eFMS.API.Report.DL.IService;
using eFMS.API.Report.DL.Models;
using eFMS.API.Report.Infrastructure.Middlewares;
using eFMS.API.Report.Service.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace eFMS.API.Report.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class SaleReportController : Controller
    {
        private readonly ISaleReportService saleReportService;
        private readonly IReportLogService reportLogService;
        public SaleReportController(ISaleReportService saleReportService, IReportLogService rptLogService)
        {
            this.saleReportService = saleReportService;
            reportLogService = rptLogService;
        }

        [HttpPost]
        [Authorize]
        public IActionResult MonthlySalereport(SaleReportCriteria criteria)
        {
            var result = saleReportService.PreviewGetMonthlySaleReport(criteria);

            Response.OnCompleted(async () =>
            {
                var reportLog = new SysReportLog
                {
                    ReportName = ReportConstants.Monthly_Sale_Report,
                    ObjectParameter = JsonConvert.SerializeObject(criteria),
                    Type = ReportConstants.Crystal_Preview
                };
                reportLogService.WriteLogReport(reportLog);
            });

            return Ok(result);

        }
    }
}