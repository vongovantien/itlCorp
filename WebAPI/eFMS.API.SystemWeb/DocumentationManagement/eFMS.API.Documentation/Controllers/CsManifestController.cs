using System;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Shipment.Infrastructure.Common;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using SystemManagementAPI.Infrastructure.Middlewares;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace eFMS.API.Documentation.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class CsManifestController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICsManifestService manifestService;
        private readonly ICurrentUser currentUser;
        public CsManifestController(IStringLocalizer<LanguageSub> localizer, ICsManifestService service, ICurrentUser user)
        {
            stringLocalizer = localizer;
            manifestService = service;
            currentUser = user;
        }

        [HttpGet]
        public IActionResult Get(Guid jobId)
        {
            var result = manifestService.GetById(jobId);
            return Ok(result);
        }

        [HttpPost]
        [Route("AddOrUpdateManifest")]
        [Authorize]
        public IActionResult AddOrUpdateManifest(CsManifestEditModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            model.UserCreated = currentUser.UserID;
            model.CreatedDate = DateTime.Now;
            var hs = manifestService.AddOrUpdateManifest(model);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost]
        [Route("PreviewFCLManifest")]
        public IActionResult PreviewFCLManifest(ManifestReportModel model)
        {
            var result = manifestService.Preview(model);
            return Ok(result);
        }
    }
}
