using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.Infrastructure.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using SystemManagementAPI.Infrastructure.Middlewares;

namespace eFMS.API.Documentation.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class TerminologyController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ITerminologyService terminologyService;
        public TerminologyController(IStringLocalizer<LanguageSub> localizer, ITerminologyService service)
        {
            stringLocalizer = localizer;
            terminologyService = service;
        }


        [HttpGet]
        [Route("GetShipmentCommonData")]
        public IActionResult Get()
        {
            var s = url;
            var results = terminologyService.GetAllShipmentCommonData();
            return Ok(results);
        }
        [HttpGet]
        [Route("GetOPSShipmentCommonData")]
        public IActionResult GetOPSCommonData()
        {
            var results = terminologyService.GetOPSShipmentCommonData();
            return Ok(results);
        }
    }
}
