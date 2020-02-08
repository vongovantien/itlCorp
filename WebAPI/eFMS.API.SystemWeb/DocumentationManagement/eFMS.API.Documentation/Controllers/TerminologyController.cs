using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.Infrastructure.AttributeEx;
using eFMS.IdentityServer.DL.IService;
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
            Guid officeId = new Guid("2FDCA3AC-6C54-434F-9D71-12F8F50B857B");
            return Ok(results);
        }
        [HttpGet]
        [Route("GetOPSShipmentCommonData")]
        public IActionResult GetOPSCommonData()
        {
            var results = terminologyService.GetOPSShipmentCommonData();
            return Ok(results);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [AuthorizeEx(Menu.acctAP, UserPermission.AllowAccess)]
        [HttpGet("Test")]
        public IActionResult GetA()
        {
            return Ok();
        }

        [AuthorizeEx(Menu.acctAP, UserPermission.Delete)]
        [HttpPost("Insert")]
        public IActionResult Insert()
        {
            return Ok();
        }
    }
}
