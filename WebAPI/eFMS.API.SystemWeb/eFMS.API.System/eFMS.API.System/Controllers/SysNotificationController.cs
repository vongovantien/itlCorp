using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using eFMS.API.Common;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.Infrastructure.Hubs;
using eFMS.API.System.Infrastructure.Middlewares;
using eFMS.API.System.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace eFMS.API.System.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class SysNotificationController : ControllerBase
    {
        private IHubContext<NotificationHub, IHubClientNotification> _signalrHub;
        private readonly ISysNotificationService sysNotificationService;
        private ICurrentUser currentUser;

        public SysNotificationController(
            IHubContext<NotificationHub, IHubClientNotification> signalrHub,
            ISysNotificationService sysNotiService,
             ICurrentUser currUser
            )
        {
            _signalrHub = signalrHub;
            sysNotificationService = sysNotiService;
            currentUser = currUser;
        }

        [HttpPost]
        [Authorize]
        public async Task<ResultHandle> Post(SysNotifications model)
        {
            ResultHandle result = new ResultHandle();
            try
            {
                HandleState hs = sysNotificationService.AddNew(model);
                if(hs.Success)
                {
                    result.Status = true;
                    result.Message = "Notification was added successfully !";
                }
            }
            catch (Exception e)
            {
                result.Message = e.ToString();
                result.Data = e.ToString();
                result.Status = false;

            }
            return result;
        }

        [HttpPost]
        [Route("SendToAllClient")]
        public async Task<IActionResult> SendToAllClient(string msg)
        {
            await _signalrHub.Clients.All.SendMessageToAllClient(msg);
            return Ok("ok");
        }

        [HttpPost]
        [Route("SendToClient")]
        public async Task<IActionResult> SendToClient(string connectionId, string msg)
        {
            await _signalrHub.Clients.Client(connectionId).SendMessageToClient(connectionId,msg);
            return Ok("ok");
        }

    }
}