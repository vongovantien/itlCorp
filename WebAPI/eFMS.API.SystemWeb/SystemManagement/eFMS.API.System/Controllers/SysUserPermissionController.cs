using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.System.DL.Common;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.Infrastructure.Common;
using eFMS.API.System.Infrastructure.Middlewares;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

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
        private readonly IStringLocalizer stringLocalizer;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="userPermission"></param>
        /// <param name="localizer"></param>
        public SysUserPermissionController(ISysUserPermissionService userPermission,
            IStringLocalizer<LanguageSub> localizer)
        {
            userPermissionService = userPermission;
            stringLocalizer = localizer;
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

        /// <summary>
        /// get detail of permission by user permission id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}")]
        public IActionResult GetBy(Guid id)
        {
            var result = userPermissionService.Get(id);
            return Ok(result);
        }

        /// <summary>
        /// get an existed item by user id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("GetByUserId")]
        public IActionResult GetByUserId(string id)
        {
            var data = userPermissionService.GetByUserId(id);
            return Ok(data);
        }


        [HttpPost("Add")]
        public IActionResult Add(List<SysUserPermissionEditModel> list)
        {
            if (!ModelState.IsValid) return BadRequest();
            var hs = userPermissionService.Add(list);
            var message = HandleError.GetMessage(hs, Crud.Insert);

            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value};
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// delete
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            var hs = userPermissionService.Delete(id);
            var message = HandleError.GetMessage(hs, Crud.Insert);

            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Update User Permission
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("Update")]
        [Authorize]
        public IActionResult Update(SysUserPermissionModel model)
        {
            if (!ModelState.IsValid) return BadRequest();

            var hs = userPermissionService.Update(model);

            var message = HandleError.GetMessage(hs, Crud.Update);

            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return Ok(result);
            }
            return Ok(result);
        }

    }
}
