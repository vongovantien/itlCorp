using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models.Criteria;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using SystemManagementAPI.Infrastructure.Middlewares;
using SystemManagementAPI.Resources;

namespace eFMS.API.Documentation.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class CsMawbcontainerController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICsMawbcontainerService csContainerService;
        public CsMawbcontainerController(IStringLocalizer<LanguageSub> localizer, ICsMawbcontainerService service)
        {
            stringLocalizer = localizer;
            csContainerService = service;
        }

        [HttpPost]
        [Route("Query")]
        public IActionResult Query(CsMawbcontainerCriteria criteria)
        {
            return Ok(csContainerService.Query(criteria));
        }
    }
}
