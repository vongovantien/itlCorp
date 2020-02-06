using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.System.DL.IService;
using eFMS.API.System.Infrastructure.Middlewares;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace eFMS.API.System.Controllers
{
    /// <summary>
    /// Controller Department
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class SysUserPermissionController : ControllerBase
    {
        private readonly ISysUserPermissionService userPermissionService;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="userPermission"></param>
        public SysUserPermissionController(ISysUserPermissionService userPermission)
        {
            userPermissionService = userPermission;
        }
        /// <summary>
        /// Get by user and office
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="officeId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetBy")]
        public IActionResult GetBy(string userId, Guid officeId)
        {
            var result = userPermissionService.GetBy(userId, officeId);
            return Ok(result);
        }
    }
}
