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
    public class SysPermissionGeneralDetailController : ControllerBase
    {
        private ISysPermissionGeneralDetailService permissionDetailService;
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="permissionDetail"></param>
        public SysPermissionGeneralDetailController(ISysPermissionGeneralDetailService permissionDetail)
        {
            permissionDetailService = permissionDetail;
        }
    }
}
