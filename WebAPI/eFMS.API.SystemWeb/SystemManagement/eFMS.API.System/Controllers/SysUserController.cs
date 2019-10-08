using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using eFMS.API.System.DL.Common;
using eFMS.API.System.Infrastructure.Middlewares;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models.Criteria;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

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

        [HttpPost]
        [Route("Paging")]
        public IActionResult Paging(SysUserCriteria criteria, int page, int size)
        {
            var data = sysUserService.Paging(criteria, page, size, out int rowCount);
            var result = new { data, totalItems = rowCount, page, size };
            return Ok(result);
        }
    }
}
