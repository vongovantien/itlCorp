using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Setting.DL.IService;
using eFMS.API.Setting.Infrastructure.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace eFMS.API.Setting.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class TariffController : ControllerBase
    {
        private readonly ITariffService tariffService;
        public TariffController(ITariffService service)
        {
            tariffService = service;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var data = tariffService.Get();
            return Ok(data);
        }

    }
}