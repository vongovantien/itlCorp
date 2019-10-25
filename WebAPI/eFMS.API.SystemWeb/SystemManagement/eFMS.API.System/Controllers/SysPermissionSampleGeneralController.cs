using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.System.DL.IService;
using eFMS.API.System.Infrastructure.Middlewares;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace eFMS.API.System.Controllers
{
    /// <summary>
    /// Controller Department
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class SysPermissionSampleGeneralController : ControllerBase
    {
        private ISysPermissionSampleGeneralService permissionDetailService;
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="permissionDetail"></param>
        public SysPermissionSampleGeneralController(ISysPermissionSampleGeneralService permissionDetail)
        {
            permissionDetailService = permissionDetail;
        }

        /// <summary>
        /// get permission detail by permission
        /// </summary>
        /// <param name="permissionId">if add new : permissionId = 0 ---/--- update: permissionId > 0</param>
        /// <returns></returns>
        [HttpGet("GetByPermission")]
        public IActionResult GetBy(short permissionId)
        {
            var results = permissionDetailService.GetBy(permissionId);
            return Ok(results);
        }
    }
}
