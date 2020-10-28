using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.System.DL.IService;
using eFMS.API.System.Infrastructure.Middlewares;
using eFMS.IdentityServer.DL.UserManager;
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
        public IActionResult Paging(int page, int size)
        {
            var data = sysUserNotificationService.Paging(page, size, out int rowCount);
            var result = new { data, totalItems = rowCount, page, size };
            return Ok(result);
        }

    }
}