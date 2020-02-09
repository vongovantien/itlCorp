using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.Infrastructure.AttributeEx;
using eFMS.IdentityServer.DL.IService;
using eFMS.IdentityServer.DL.UserManager;
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
        ICurrentUser currentUser;
        public TerminologyController(IStringLocalizer<LanguageSub> localizer, ITerminologyService service,
            ICurrentUser currUser)
        {
            stringLocalizer = localizer;
            terminologyService = service;
            currentUser = currUser;
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
            var user = currentUser;
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
