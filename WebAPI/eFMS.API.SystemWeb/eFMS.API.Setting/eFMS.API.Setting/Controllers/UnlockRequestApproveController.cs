using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Common.Globals;
using eFMS.API.Setting.DL.IService;
using eFMS.API.Setting.Infrastructure.Middlewares;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace eFMS.API.Setting.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class UnlockRequestApproveController : ControllerBase
    {
        readonly ICurrentUser currentUser;
        private readonly IUnlockRequestApproveService unlockRequestApproveService;
        private readonly IStringLocalizer stringLocalizer;
        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="service"></param>
        /// <param name="currUser"></param>
        public UnlockRequestApproveController(IStringLocalizer<LanguageSub> localizer, IUnlockRequestApproveService service, ICurrentUser currUser)
        {
            stringLocalizer = localizer;
            unlockRequestApproveService = service;
            currentUser = currUser;
        }

        [HttpGet("Get")]
        public IActionResult Get()
        {
            return Ok(unlockRequestApproveService.Get());
        }

        [HttpGet]
        [Route("GetInfoApproveUnlockRequest")]
        //[Authorize]
        public IActionResult GetInfoApproveUnlockRequest(Guid id)
        {
            var data = unlockRequestApproveService.GetInfoApproveUnlockRequest(id);
            return Ok(data);
        }

        [HttpGet]
        [Route("GetHistoryDenied")]
        //[Authorize]
        public IActionResult GetHistoryDenied(Guid id)
        {
            var data = unlockRequestApproveService.GetHistoryDenied(id);
            return Ok(data);
        }
    }
}