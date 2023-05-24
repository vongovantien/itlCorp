using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SystemManagementAPI.Infrastructure.Middlewares;

namespace eFMS.API.Documentation.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class CsStageAssignedController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICsStageAssignedService csStageAssignedService;
        private readonly IStageService stageService;
        private readonly ICurrentUser currentUser;

        public CsStageAssignedController(IStringLocalizer<LanguageSub> localizer, ICsStageAssignedService csStageAssigned, ICurrentUser user, IStageService stage)
        {
            csStageAssignedService = csStageAssigned;
            stageService = stage;
            currentUser = user;
            stringLocalizer = localizer;
        }

        [HttpPost]
        [Route("AddNewStageByEventType")]
        [Authorize]
        public async Task<IActionResult> AddNewStageByType(CsStageAssignedCriteria criteria)
        {
            var hs = await csStageAssignedService.AddNewStageAssignedByType(criteria);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            return Ok(result);
        }

        [HttpPost]
        [Route("AddMultipleStage")]
        [Authorize]
        public async Task<IActionResult> AddMultipleStage(Guid jobID, [FromBody] List<CsStageAssignedModel> listItem)
        {
            foreach (var item in listItem)
            {
                item.Status = TermData.InSchedule;
                if(string.IsNullOrEmpty(item.Type)) {
                    item.Type = DocumentConstants.FROM_USER;
                }
                item.RealPersonInCharge = item.MainPersonInCharge;
                item.UserCreated = item.UserModified = currentUser.UserID;
            }
            var hs = await csStageAssignedService.AddMultipleStageAssigned(jobID, listItem);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };

            return Ok(result);
        }
    }
}
