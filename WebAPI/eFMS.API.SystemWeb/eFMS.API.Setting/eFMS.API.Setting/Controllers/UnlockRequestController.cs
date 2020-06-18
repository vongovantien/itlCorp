using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.API.Setting.DL.IService;
using eFMS.API.Setting.DL.Models;
using eFMS.API.Setting.DL.Models.Criteria;
using eFMS.API.Setting.Infrastructure.Middlewares;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace eFMS.API.Setting.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class UnlockRequestController : ControllerBase
    {
        readonly ICurrentUser currentUser;
        private readonly IUnlockRequestService unlockRequestService;
        private readonly IStringLocalizer stringLocalizer;
        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="service"></param>
        /// <param name="currUser"></param>
        public UnlockRequestController(IStringLocalizer<LanguageSub> localizer, IUnlockRequestService service, ICurrentUser currUser)
        {
            stringLocalizer = localizer;
            unlockRequestService = service;
            currentUser = currUser;
        }

        [HttpGet()]
        public IActionResult Get()
        {
            return Ok(unlockRequestService.Get());
        }

        [HttpPost]
        [Route("Add")]
        [Authorize]
        public IActionResult AddUnlockRequest(SetUnlockRequestModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var hs = unlockRequestService.AddUnlockRequest(model);

            var message = HandleError.GetMessage(hs, Crud.Insert);

            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = model };
            return Ok(result);
        }

        [HttpDelete]
        [Route("Delete")]
        [Authorize]
        public IActionResult Delete(Guid id)
        {
            var hs = unlockRequestService.DeleteUnlockRequest(id);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            return Ok(result);
        }

        [HttpPut]
        [Route("Update")]
        [Authorize]
        public IActionResult UpdateUnlockRequest(SetUnlockRequestModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var hs = unlockRequestService.UpdateUnlockRequest(model);

            var message = HandleError.GetMessage(hs, Crud.Update);

            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = model };
            return Ok(result);
        }

        [HttpPost("GetJobToUnlockRequest")]
        public IActionResult GetJobToUnlockRequest(UnlockJobCriteria criteria)
        {
            var data = unlockRequestService.GetJobToUnlockRequest(criteria);
            return Ok(data);
        }
    }
}