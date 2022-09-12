using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Common;
using eFMS.API.System.DL.IService;
using eFMS.API.System.Infrastructure.Middlewares;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace eFMS.API.System.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class SysUserNotificationController : ControllerBase
    {
        private readonly ICurrentUser currentUser;
        private readonly ISysUserNotification sysUserNotificationService;

        public SysUserNotificationController(
            ICurrentUser currentUser, 
            ISysUserNotification sysUserNotificationService)
        {
            this.currentUser = currentUser;
            this.sysUserNotificationService = sysUserNotificationService;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var response = sysUserNotificationService.GetAll();
            return Ok(response);
        }

        [HttpGet]
        [Route("Paging")]
        [Authorize]
        public IActionResult Paging(int page, int size)
        {
            var data = sysUserNotificationService.Paging(page, size, out int rowCount, out int totalNoRead);
            var result = new { data, totalItems = rowCount, page, size, totalNoRead };
            return Ok(result);
        }

        [HttpPut]
        [Authorize]
        [Route("Read")]
        public IActionResult ReadMessage(Guid Id)
        {
            HandleState result = sysUserNotificationService.Update(Id);
            if(result.Success)
            {
                return Ok(new ResultHandle { Message = "Đọc tin nhắn thành công", Status = true});
            }

            return BadRequest(new ResultHandle { Message = "Đọc tin nhắn không thành công", Status = true });
        }

        [HttpDelete]
        [Route("Delete")]
        [Authorize]
        public IActionResult DeleteMessage(Guid Id)
        {
            HandleState result = sysUserNotificationService.Delete(x => x.Id == Id);
            if (result.Success)
            {
                return Ok(new ResultHandle { Message = "Notify was deleted successfully", Status = true });
            }

            return BadRequest(new ResultHandle { Message = "Delete notify fail", Status = true });
        }
    }
}