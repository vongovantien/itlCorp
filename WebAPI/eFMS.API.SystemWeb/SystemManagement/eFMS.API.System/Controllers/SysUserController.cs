using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
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
    public class SysUserController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ISysUserService sysUserService;
        private readonly IMapper mapper;
        public SysUserController(IStringLocalizer<LanguageSub> localizer, ISysUserService service, IMapper iMapper)
        {
            stringLocalizer = localizer;
            sysUserService = service;
            mapper = iMapper;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var results = sysUserService.GetAll();
            return Ok(results);
        }
    }
}