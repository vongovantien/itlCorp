using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        private readonly ICsStageAssignedService csStageAssignedService;
        private readonly IStageService stageService;
        private readonly ICurrentUser currentUser;

        public CsStageAssignedController(ICsStageAssignedService csStageAssigned, ICurrentUser user, IStageService stage)
        {
            csStageAssignedService = csStageAssigned;
            stageService = stage;
            currentUser = user;
        }

        [HttpPost]
        [Route("AddNewStageByEventType")]
        [Authorize]
        public async Task<IActionResult> AddNewStageByType(CsStageAssignedCriteria criteria)
        {
            var status = await csStageAssignedService.AddNewStageAssignedByType(criteria);
            return Ok(status);
        }

        [HttpPost]
        [Route("AddMultipleStage")]
        [Authorize]
        public async Task<IActionResult> AddMutipleStage(Guid jobID, [FromBody] List<CsStageAssignedModel> listItem)
        {
            foreach (var item in listItem)
            {
                item.Deadline = DateTime.Now;
                item.Status = TermData.Done;
                item.MainPersonInCharge = item.RealPersonInCharge = currentUser.UserID;
            }
            var result = await csStageAssignedService.AddMultipleStageAssigned(jobID, listItem);

            return Ok(result);
        }
    }
}
