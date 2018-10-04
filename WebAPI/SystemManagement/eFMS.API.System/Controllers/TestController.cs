using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using SystemManagementAPI.Infrastructure.Middlewares;
using SystemManagementAPI.Resources;

namespace eFMS.API.System.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        public TestController(IStringLocalizer<LanguageSub> localizer)
        {
            stringLocalizer = localizer;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(stringLocalizer[LanguageSub.MSG_DATA_NOT_FOUND]);
        }
    }
}