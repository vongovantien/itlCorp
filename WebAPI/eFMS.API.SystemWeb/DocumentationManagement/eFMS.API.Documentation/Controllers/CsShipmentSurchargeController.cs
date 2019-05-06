using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.Service.Models;
using eFMS.API.Shipment.Infrastructure.Common;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Authorization;
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
    public class CsShipmentSurchargeController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICsShipmentSurchargeService csShipmentSurchargeService;
        private readonly ICurrentUser currentUser;
        public CsShipmentSurchargeController(IStringLocalizer<LanguageSub> localizer,ICsShipmentSurchargeService service, ICurrentUser user)
        {
            stringLocalizer = localizer;
            csShipmentSurchargeService = service;
            currentUser = user;
        }

        [HttpGet]
        [Route("GetByHB")]
        public IActionResult GetByHouseBill(Guid hbId,string type)
        {

            return Ok(csShipmentSurchargeService.GetByHB(hbId,type));
        }

        [HttpGet]
        [Route("GroupByListHB")]
        public List<object> GetByListHouseBill(Guid JobId,string partnerID,bool getAll=false)
        {

            return csShipmentSurchargeService.GroupChargeByHB(JobId, partnerID,getAll);
        }

        [HttpGet]
        [Route("GetPartnersByJob")]
        public List<CatPartner> GetPartnersByListHB(Guid JobId)
        {

            return csShipmentSurchargeService.GetAllParnerByJob(JobId);
        }


        [HttpPost]
        [Route("Add")]
        [Authorize]
        public IActionResult AddNew(CsShipmentSurchargeModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            model.UserCreated = currentUser.UserID;
            var hs = csShipmentSurchargeService.Add(model);           
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPut]
        [Route("Update")]
        [Authorize]
        public IActionResult Update(CsShipmentSurchargeModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            model.UserModified = currentUser.UserID;
            model.DatetimeModified = DateTime.Now;
            var hs = csShipmentSurchargeService.Update(model, x => x.Id == model.Id);            
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);

        }

        [HttpDelete]
        [Route("Delete")]
        //[Authorize]
        public IActionResult Delete(Guid chargId)
        {
            var hs = csShipmentSurchargeService.DeleteCharge(chargId);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}