using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Documentation.DL.IService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using SystemManagementAPI.Resources;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace eFMS.API.Documentation.Controllers
{
    /// <summary>
    /// A base class for an MVC controller without view support.
    /// </summary>
    public class OpsStageAssignedController : Controller
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly IOpsStageAssignedService opsStageAssignedService;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="localizer">inject interface IStringLocalizer</param>
        /// <param name="opsStageAssigned"></param>
        public OpsStageAssignedController(IStringLocalizer<LanguageSub> localizer, IOpsStageAssignedService opsStageAssigned)
        {
            stringLocalizer = localizer;
            opsStageAssignedService = opsStageAssigned;
        }

        /// <summary>
        /// get all data
        /// </summary>
        /// <returns></returns>
        public IActionResult Get()
        {
            var results = opsStageAssignedService.Get();
            return Ok(results);
        }
    }
}
