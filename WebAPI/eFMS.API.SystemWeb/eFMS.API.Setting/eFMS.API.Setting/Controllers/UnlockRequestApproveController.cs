using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Common;
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
        [Authorize]
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

        [HttpPost]
        [Route("UpdateApprove")]
        [Authorize]
        public IActionResult UpdateApprove(Guid id)
        {
            var updateApproval = unlockRequestApproveService.UpdateApproval(id);
            ResultHandle _result;
            if (!updateApproval.Success)
            {
                _result = new ResultHandle { Status = updateApproval.Success, Message = updateApproval.Exception.Message };
                return BadRequest(_result);
            }
            else
            {
                _result = new ResultHandle { Status = updateApproval.Success };
                return Ok(_result);
            }
        }

        [HttpPost]
        [Route("DeniedApprove")]
        [Authorize]
        public IActionResult DeniedApprove(Guid id, string comment)
        {
            var denieApproval = unlockRequestApproveService.DeniedApprove(id, comment);
            ResultHandle _result;
            if (!denieApproval.Success)
            {
                _result = new ResultHandle { Status = denieApproval.Success, Message = denieApproval.Exception.Message };
                return BadRequest(_result);
            }
            else
            {
                _result = new ResultHandle { Status = denieApproval.Success };
                return Ok(_result);
            }
        }

        [HttpPost]
        [Route("CancelRequest")]
        [Authorize]
        public IActionResult CancelRequest(Guid id)
        {
            var updateApproval = unlockRequestApproveService.CancelRequest(id);
            ResultHandle _result;
            if (!updateApproval.Success)
            {
                _result = new ResultHandle { Status = updateApproval.Success, Message = updateApproval.Exception.Message };
                return BadRequest(_result);
            }
            else
            {
                _result = new ResultHandle { Status = updateApproval.Success };
                return Ok(_result);
            }
        }
    }
}