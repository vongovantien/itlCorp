using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Infrastructure.Extensions;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SystemManagementAPI.Infrastructure.Middlewares;

namespace eFMS.API.Documentation.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class CsLinkChargeController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICsLinkChargeService csLinkChargeService;
        private ICurrentUser currentUser;
        public CsLinkChargeController(ICsLinkChargeService service,
            ICurrentUser current,
            IStringLocalizer<LanguageSub> localizer)
        {
            currentUser = current;
            stringLocalizer = localizer;
            csLinkChargeService = service;
        }

        [HttpPost("UpdateChargeLinkFee")]
        [Authorize]
        public IActionResult UpdateChargeLinkFee([FromBody] List<CsShipmentSurchargeModel> list)
        {

            currentUser.Action = DateTime.Now.ToString("DD/MM/yyyy")+"-"+currentUser.UserName+"-"+"UpdateChargeLinkFee";
            var hs = csLinkChargeService.UpdateChargeLinkFee(list);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPost("RevertChargeLinkFee")]
        [Authorize]
        public IActionResult RevertChargeLinkFee([FromBody] List<CsShipmentSurchargeModel> list)
        {
            currentUser.Action = DateTime.Now.ToString("DD/MM/yyyy") + "-" + currentUser.UserName + "-" + "RevertChargeLinkFee";
            var hs = csLinkChargeService.RevertChargeLinkFee(list);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpGet("DetailByChargeOrgId")]
        public IActionResult DetailByChargeOrgId(Guid id)
        {
            currentUser.Action = DateTime.Now.ToString("DD/MM/yyyy") + "-" + currentUser.UserName + "-" + "DetailByChargeOrgId";
            var result = csLinkChargeService.DetailByChargeOrgId(id);
            if (result == null) return Ok();
            return Ok(result);
        }

        [HttpPost("LinkFeeJob")]
        [Authorize]
        public IActionResult LinkFeeJob([FromBody] List<OpsTransactionModel> list)
        {
            currentUser.Action = DateTime.Now.ToString("DD/MM/yyyy") + "-" + currentUser.UserName + "-" + "LinkFeeJob";

            HandleState hs = csLinkChargeService.LinkFeeJob(list);
            string message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = null };
            if (!hs.Success)
                return BadRequest(result);
            return Ok(result);
        }
    }
}
