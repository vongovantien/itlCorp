using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.Infrastructure.Middlewares;
using eFMS.API.System.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace eFMS.API.System.Controllers
{

    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class SysSettingFlowController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly IMapper Imapper;
        private readonly ICurrentUser currentUser;
        private readonly ISysSettingFlowService sysSettingFlowService;

        public SysSettingFlowController(IMapper mapper,IStringLocalizer<LanguageSub> localizer, ICurrentUser currUser, ISysSettingFlowService service)
        {
            Imapper = mapper;
            stringLocalizer = localizer;
            sysSettingFlowService = service;
            currentUser = currUser;

        }

        [HttpGet("GetSettingFlowByOffice")]
        public IActionResult GetSettingFlowByOffice(Guid officeId)
        {
            return Ok(sysSettingFlowService.GetByOfficeId(officeId));
        }

        [HttpPost]
        [Route("Add")]
        [Authorize]
        public IActionResult Post(SysSettingFlowEditModel model)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest();
            }
            HandleState hs = sysSettingFlowService.UpdateSettingFlow(model.settings,model.OfficeId);

            string message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = null };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}