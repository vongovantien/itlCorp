using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.System.DL.Common;
using eFMS.API.System.DL.IService;
using eFMS.API.System.Infrastructure.Middlewares;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace eFMS.API.System.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class CatDepartmentController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICatDepartmentService catDepartmentService;
        public CatDepartmentController(IStringLocalizer<LanguageSub> localizer, ICatDepartmentService service)
        {
            stringLocalizer = localizer;
            catDepartmentService = service;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(catDepartmentService.Get());

        }

    }

}
