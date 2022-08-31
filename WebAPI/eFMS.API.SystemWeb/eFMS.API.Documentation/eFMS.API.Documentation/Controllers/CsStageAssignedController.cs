using eFMS.API.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System;
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
        private readonly ICurrentUser currentUser;

        public CsStageAssignedController(ICsStageAssignedService csStageAssigned, ICurrentUser user)
        {
            csStageAssignedService = csStageAssigned;
            currentUser = user;
        }

        [HttpPost]
        [Route("AddNewStageByTransactionType")]
        [Authorize]
        public IActionResult AddNewStageByTransactionType(CsStageAssignedCriteria criteria)
        {
            var status = csStageAssignedService.AddNewStageAssignedByTransactionType(criteria);
            return Ok(status);
        }
    }
}
