using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.System.DL.IService;
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
        private readonly ICatBranchService catbranchService;
        public TestController(IStringLocalizer<LanguageSub> localizer, ICatBranchService service)
        {
            stringLocalizer = localizer;
            catbranchService = service;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(stringLocalizer[LanguageSub.MSG_DATA_NOT_FOUND]);
        }

        [HttpGet("{id}")]
        public IActionResult Get(Guid id)
        {
            var s = catbranchService.Any(x => x.Id == id);
            var t = catbranchService.First(x => x.Id == id);
            var result = catbranchService.Get();
            return Ok();
        }
    }
}