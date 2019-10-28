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
{
    /// <summary>
    /// Controller Department
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class SysPermissionSampleSpecialController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ISysPermissionSampleSpecialService perSpecialServiceRepository;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="peSpecialServiceRepoy"></param>
        public SysPermissionSampleSpecialController(IStringLocalizer<LanguageSub> localizer,
            ISysPermissionSampleSpecialService peSpecialServiceRepoy)
        {
            stringLocalizer = localizer;
            perSpecialServiceRepository = peSpecialServiceRepoy;
        }

        /// <summary>
        /// get list by permission Id
        /// </summary>
        /// <param name="permissionId"></param>
        /// <returns></returns>
        [HttpGet("GetByPermission")]
        public IActionResult GetBy(Guid? permissionId)
        {
            var results = perSpecialServiceRepository.GetBy(permissionId);
            return Ok(results);
        }
    }
}
