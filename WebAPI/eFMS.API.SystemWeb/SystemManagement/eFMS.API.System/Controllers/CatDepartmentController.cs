
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models.Criteria;
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