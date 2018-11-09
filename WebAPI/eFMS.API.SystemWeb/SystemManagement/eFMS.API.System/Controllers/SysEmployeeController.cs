using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
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
    public class SysEmployeeController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ISysEmployeeService sysEmployeeService;
        private readonly IMapper mapper;
        public SysEmployeeController(IStringLocalizer<LanguageSub> localizer, ISysEmployeeService service, IMapper iMapper)
        {
            stringLocalizer = localizer;
            sysEmployeeService = service;
            mapper = iMapper;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var results = sysEmployeeService.Get();
            return Ok(results);
        }
        [HttpPost]
        [Route("Query")]
        public IActionResult Query(EmployeeCriteria criteria)
        {
            var results = sysEmployeeService.Query(criteria);
            return Ok(results);
        }
    }
}