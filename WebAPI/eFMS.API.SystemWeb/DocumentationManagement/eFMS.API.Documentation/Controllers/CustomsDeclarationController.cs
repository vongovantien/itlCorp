using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Documentation.DL.IService;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using SystemManagementAPI.Infrastructure.Middlewares;
using SystemManagementAPI.Resources;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace eFMS.API.Documentation.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class CustomsDeclarationController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICustomsDeclarationService customclearanceService;
        private readonly ICurrentUser currentUser;

        public CustomsDeclarationController(IStringLocalizer<LanguageSub> localizer, ICurrentUser user, ICustomsDeclarationService customclearance)
        {
            stringLocalizer = localizer;
            currentUser = user;
            customclearanceService = customclearance;
        }
        [HttpGet]
        [Route("GetByJob")]
        public IActionResult GetByJob(Guid jobId)
        {
            return Ok(customclearanceService.GetByJobId(jobId));
        }
    }
}
