using System;
using eFMS.API.Catalogue.Authorize;
using eFMS.API.Catalogue.DL.Common;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.Infrastructure.Middlewares;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace eFMS.API.Catalogue.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class TestController : ControllerBase
    {
        public TestController()
        {

        }
        
        [AuthorizeEx("ABC")]
        [HttpGet]
        public IActionResult Get()
        {
            return Ok();
        }
    }
}