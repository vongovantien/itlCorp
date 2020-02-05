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
{/// <summary>
 /// Controller Department
 /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class SysRoleController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ISysRoleService roleService;
        private readonly ISysMenuService menuService;
        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="service"></param>
        public SysRoleController(IStringLocalizer<LanguageSub> localizer, ISysRoleService service, ISysMenuService menu)
        {
            stringLocalizer = localizer;
            roleService = service;
            menuService = menu;
        }

        /// <summary>
        /// get all role
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Get()
        {
            var results = roleService.Get();
            return Ok(results);
        }

        [HttpGet("GetMenus")]
        public IActionResult GetMenus()
        {
            var results = menuService.GetMenus();
            return Ok(results);
        }
    }
}
