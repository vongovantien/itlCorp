using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models.Criteria;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using SystemManagementAPI.Infrastructure.Middlewares;
using SystemManagementAPI.Resources;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace eFMS.API.Documentation.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class CsTransactionDetailController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICsTransactionDetailService csTransactionDetailService;
        public CsTransactionDetailController(IStringLocalizer<LanguageSub> localizer, ICsTransactionDetailService service)
        {
            stringLocalizer = localizer;
            csTransactionDetailService = service;
        }

        [HttpGet]
        [Route("GetByJob")]
        public IActionResult GetByJob(Guid jobId)
        {
            CsTransactionDetailCriteria criteria = new CsTransactionDetailCriteria { JobId = jobId };
            return Ok(csTransactionDetailService.GetByJob(criteria));
        }
    }
}
