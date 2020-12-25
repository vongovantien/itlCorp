using System;
using System.Collections.Generic;
using System.Linq;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.API.System.DL.Common;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.Infrastructure.Middlewares;
using eFMS.IdentityServer.DL.UserManager;
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
        private readonly ICurrentUser currentUser;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="userPermission"></param>
        /// <param name="localizer"></param>
        public SysUserPermissionController(ISysUserPermissionService userPermission,
            IStringLocalizer<LanguageSub> localizer,
            ICurrentUser currUser)
        {
            userPermissionService = userPermission;
            stringLocalizer = localizer;
            currentUser = currUser;
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
        /// get permission by current user + route
        /// </summary>
        /// <param name="route"></param>
        /// <returns></returns>
        [HttpGet("Permissions/{route}")]
        [Authorize]
        public IActionResult Permissions(string route)
        {
            var result = userPermissionService.GetPermission(currentUser.UserID, currentUser.OfficeID, route);
            if (result == null) return Forbid();
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
            var checkDupRole = list.GroupBy(x => new { x.OfficeId })
                                .Where(t => t.Count() > 1)
                                .Select(y => y.Key)
                                .ToList();
            if (checkDupRole.Count > 0)
            {
                return Ok(new ResultHandle { Status = false, Message = stringLocalizer[SystemLanguageSub.MSG_ITEM_DUPLICATE_ROLE_ON_USER, "role"].Value, Data = checkDupRole });
            }

            var hs = userPermissionService.Add(list);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
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
            var message = HandleError.GetMessage(hs, Crud.Delete);

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
