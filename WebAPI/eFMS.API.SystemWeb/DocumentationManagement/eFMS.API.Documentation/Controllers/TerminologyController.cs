using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.Infrastructure.AttributeEx;
using eFMS.IdentityServer.DL.IService;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Authorization;
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
    public class TerminologyController : CustomBaseController//ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ITerminologyService terminologyService;
        //public TerminologyController(IStringLocalizer<LanguageSub> localizer, ITerminologyService service,
        //    ICurrentUser currUser)
        //{
        //    stringLocalizer = localizer;
        //    terminologyService = service;
        //    currentUser = currUser;
        //}

        public TerminologyController(ICurrentUser currentUser, IStringLocalizer<LanguageSub> localizer, ITerminologyService service, Menu menu = Menu.acctAP) : base(currentUser, menu)
        {
            stringLocalizer = localizer;
            terminologyService = service;
            var s = currentUser;
        }

        [HttpGet]
        [Route("GetShipmentCommonData")]
        [Authorize]
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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [AuthorizeEx(Menu.acctAP, UserPermission.AllowAccess)]
        [HttpGet("Test")]
        public IActionResult GetA()
        {
            // var user = currentUser;
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
