using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Operation.DL.Common;
using eFMS.API.Operation.DL.IService;
using eFMS.API.Operation.DL.Models;
using eFMS.API.Operation.Infrastructure.Common;
using eFMS.API.Operation.Infrastructure.Middlewares;
using eFMS.API.Operation.Resources;
using eFMS.API.Operation.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace eFMS.API.Operation.Controllers
{
    /// <summary>
    /// A base class for an MVC controller without view support.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class OpsStageAssignedController : Controller
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly IOpsStageAssignedService opsStageAssignedService;
        private readonly IMapper mapper;
        private readonly ICurrentUser currentUser;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="service"></param>
        /// <param name="iMapper"></param>
        /// <param name="user"></param>
        public OpsStageAssignedController(IStringLocalizer<LanguageSub> localizer, 
            IOpsStageAssignedService service, 
            IMapper iMapper,
            ICurrentUser user)
        {
            stringLocalizer = localizer;
            opsStageAssignedService = service;
            mapper = iMapper;
            currentUser = user;
        }

        /// <summary>
        /// get list of stages that assigned to a job
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        [HttpGet("GetBy")]
        public IActionResult Get(Guid jobId)
        {
            var results = opsStageAssignedService.GetByJob(jobId);
            return Ok(results);
        }

        /// <summary>
        /// get stage assigned
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public IActionResult GetById(Guid id)
        {
            var result = opsStageAssignedService.GetBy(id);
            return Ok(result);
        }

        /// <summary>
        /// get list of stages that not assigned to a job
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        [HttpGet("GetNotAssigned")]
        public IActionResult GetNotAssigned(Guid jobId)
        {
            var results = opsStageAssignedService.GetNotAssigned(jobId);
            return Ok(results);
        }

        /// <summary>
        /// add list of stages to a job
        /// </summary>
        /// <param name="models">list of stages</param>
        /// <param name="jobId">id of job want to add list stages</param>
        /// <returns></returns>
        [Authorize]
        [HttpPut("AddMultipleStage")]
        public IActionResult AddMultipleStage(List<OpsStageAssignedEditModel> models, Guid jobId)
        {
            var hs = opsStageAssignedService.AddMultipleStage(models, jobId);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// get status of stage
        /// </summary>
        /// <returns></returns>
        [HttpGet("OperatiosStageStatus")]
        public IActionResult OperatiosStageStatus()
        {
            var results = Constants.OperationStages;
            return Ok(results);
        }

        /// <summary>
        /// update an existed item
        /// </summary>
        /// <param name="model">model to update</param>
        /// <returns></returns>
        [HttpPut("Update")]
        [Authorize]
        public IActionResult Update(OpsStageAssignedEditModel model)
        {
            var assigned = mapper.Map<OpsStageAssignedModel>(model);
            assigned.UserModified = currentUser.UserID;
            assigned.ModifiedDate = DateTime.Now;
            var hs = opsStageAssignedService.Update(assigned, x => x.Id == model.Id);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}
