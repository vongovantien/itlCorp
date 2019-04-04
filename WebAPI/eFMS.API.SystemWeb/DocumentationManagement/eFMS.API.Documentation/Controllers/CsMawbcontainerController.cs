using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Shipment.Infrastructure.Common;
using eFMS.IdentityServer.DL.UserManager;
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
        private readonly ICurrentUser currentUser;
        public CsMawbcontainerController(IStringLocalizer<LanguageSub> localizer, ICsMawbcontainerService service, ICurrentUser user)
        {
            stringLocalizer = localizer;
            csContainerService = service;
            currentUser = user;
        }

        [HttpPost]
        [Route("Query")]
        public IActionResult Query(CsMawbcontainerCriteria criteria)
        {
            return Ok(csContainerService.Query(criteria));
        }
        [HttpPut]
        [Route("Update")]
        public IActionResult Update(CsMawbcontainerEdit model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var hs = csContainerService.Update(model.CsMawbcontainerModels, model.MasterId, model.HousebillId);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}
