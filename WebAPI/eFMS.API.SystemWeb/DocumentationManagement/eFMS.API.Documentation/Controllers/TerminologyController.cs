using eFMS.API.Documentation.DL.IService;
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
