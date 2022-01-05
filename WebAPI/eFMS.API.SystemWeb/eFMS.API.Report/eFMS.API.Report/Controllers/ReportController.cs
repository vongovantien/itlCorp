using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Common.Globals;
using eFMS.API.Report.DL.IService;
using eFMS.API.Report.Infrastructure.Middlewares;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace eFMS.API.Report.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly IReportDocumentService reportDocService;
        private readonly IStringLocalizer stringLocalizer;
        public ReportController(IReportDocumentService reportDocService,
            IStringLocalizer<LanguageSub> localizer)
        {
            this.reportDocService = reportDocService;
            this.stringLocalizer = localizer;

        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            var a = reportDocService.Query();

            return Ok(new { a });
        }
    }
}
