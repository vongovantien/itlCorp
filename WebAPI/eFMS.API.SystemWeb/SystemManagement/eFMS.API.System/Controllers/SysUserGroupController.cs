using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.System.DL.IService;
using eFMS.API.System.Infrastructure.Middlewares;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace eFMS.API.System.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class SysUserGroupController : Controller
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ISysUserGroupService userGroupService;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="sysUserGroup"></param>
        public SysUserGroupController(ISysUserGroupService sysUserGroup)
        {
            userGroupService = sysUserGroup;
        }

        /// <summary>
        /// get by group id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("GetByGroup/{id}")]
        public IActionResult GetByGroup(short id)
        {
            var results = userGroupService.GetByGroup(id);
            return Ok(results);
        }
    }
}
