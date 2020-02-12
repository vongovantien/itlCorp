using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.Infrastructure.AttributeEx;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
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
        private ICurrentUser curUser;
        public TerminologyController(ITerminologyService service,
            ICurrentUser currentUser,
            IStringLocalizer<LanguageSub> localizer)
        {
            curUser = currentUser;
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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [AuthorizeEx(Menu.acctAP, UserPermission.AllowAccess)]
        [HttpGet("Test")]
        public IActionResult GetA()
        {
            curUser = PermissionEx.GetUserMenuPermission(curUser, Menu.acctAP);
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
