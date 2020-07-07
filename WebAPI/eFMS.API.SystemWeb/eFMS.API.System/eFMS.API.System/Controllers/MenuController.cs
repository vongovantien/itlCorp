using System;
using System.Collections.Generic;
using eFMS.API.Common.Globals;
using eFMS.API.System.DL.IService;
using eFMS.API.System.Infrastructure.Middlewares;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace eFMS.API.System.Controllers
{
    /// <summary>
    /// Controller Menu
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class MenuController : ControllerBase
    {
        private readonly ISysMenuService menuService;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="menu"></param>
        public MenuController(ISysMenuService menu)
        {
            menuService = menu;
        }

        /// <summary>
        /// get menus by user and office
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="officeId"></param>
        /// <returns></returns>
        [HttpGet("GetMenus")]
        public IActionResult GetMenus(string userId, Guid officeId)
        {
            var results = menuService.GetMenus(userId, officeId);
            return Ok(results);
        }

        /// <summary>
        /// get list menus service
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetListService")]
        [Authorize]
        public IActionResult GetListService()
        {
            List<CommonData> result = menuService.GetListService();
            return Ok(result);
        }


    }
}
