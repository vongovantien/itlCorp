using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Operation.DL.IService;
using eFMS.API.Operation.Infrastructure.Middlewares;
using eFMS.API.Operation.Resources;
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

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="service"></param>
        public OpsStageAssignedController(IStringLocalizer<LanguageSub> localizer, IOpsStageAssignedService service)
        {
            stringLocalizer = localizer;
            opsStageAssignedService = service;
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
    }
}
