using System;
using System.Collections.Generic;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.Common;
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

namespace eFMS.API.Documentation.Controllers
{
    /// <summary>
    /// A base class for an MVC controller without view support.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class CsShipmentSurchargeController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICsShipmentSurchargeService csShipmentSurchargeService;
        private readonly ICurrentUser currentUser;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="service"></param>
        /// <param name="user"></param>
        public CsShipmentSurchargeController(IStringLocalizer<LanguageSub> localizer,ICsShipmentSurchargeService service, ICurrentUser user)
        {
            stringLocalizer = localizer;
            csShipmentSurchargeService = service;
            currentUser = user;
        }

        /// <summary>
        /// get list of surcharge by house bill and type
        /// </summary>
        /// <param name="hbId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetByHB")]
        public IActionResult GetByHouseBill(Guid hbId,string type)
        {

            return Ok(csShipmentSurchargeService.GetByHB(hbId,type));
        }

        /// <summary>
        /// get list of surcharge by house bill anf partner id
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="partnerID"></param>
        /// <param name="IsHouseBillID"></param>
        /// <param name="isAddCDNote"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GroupByListHB")]
        public List<object> GetByListHouseBill(Guid Id,string partnerID,bool IsHouseBillID)
        {

            return csShipmentSurchargeService.GroupChargeByHB(Id, partnerID,IsHouseBillID);
        }

        /// <summary>
        /// get partners have surcharge
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="IsHouseBillID"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetPartners")]
        public List<CatPartner> GetPartners(Guid Id,bool IsHouseBillID)
        {

            return csShipmentSurchargeService.GetAllParner(Id, IsHouseBillID);
        }

        /// <summary>
        /// add new surcharge
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
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

        /// <summary>
        /// update an existed surcharge
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
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

        /// <summary>
        /// delete an existed surcharge
        /// </summary>
        /// <param name="chargId"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Get list charge shipment by conditions
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("ListChargeShipment")]
        public ChargeShipmentResult ListChargeShipment(ChargeShipmentCriteria criteria)
        {
            var data = csShipmentSurchargeService.GetListChargeShipment(criteria);
            return data;
        }
    }
}