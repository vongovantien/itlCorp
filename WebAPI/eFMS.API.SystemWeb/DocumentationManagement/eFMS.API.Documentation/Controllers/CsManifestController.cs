using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Shipment.Infrastructure.Common;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using SystemManagementAPI.Infrastructure.Middlewares;
using SystemManagementAPI.Resources;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace eFMS.API.Documentation.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class CsManifestController : Controller
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

        [HttpGet("id")]
        public IActionResult Get(Guid id)
        {
            var result = manifestService.Get(x => x.JobId == id).FirstOrDefault();
            return Ok(result);
        }

        [HttpPost]
        [Route("Add")]
        //[Authorize]
        public IActionResult AddManifest(CsManifestEditModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            //model.CsManifest.UserCreated = currentUser.UserID;
            //model.CsManifest.CreatedDate = DateTime.Now;
            var hs = manifestService.AddNewManifest(model);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}
