using System;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using SystemManagementAPI.Infrastructure.Middlewares;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace eFMS.API.Documentation.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class CsArrivalDeliveryOrderController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICsArrivalFrieghtChargeService arrivalFreightChargeServices;
        private readonly ICurrentUser currentUser;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="freightChargeService"></param>
        /// <param name="user"></param>
        public CsArrivalDeliveryOrderController(IStringLocalizer<LanguageSub> localizer, ICsArrivalFrieghtChargeService freightChargeService, ICurrentUser user)
        {
            stringLocalizer = localizer;
            arrivalFreightChargeServices = freightChargeService;
            currentUser = user;
        }

        /// <summary>
        /// get arrival by housebill id
        /// </summary>
        /// <param name="hblid"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpGet("GetArrival")]
        [Authorize]
        public IActionResult GetArrival(Guid hblid, TransactionTypeEnum type)
        {
            string transactionType = DataTypeEx.GetType(type);
            var result = arrivalFreightChargeServices.GetArrival(hblid, transactionType);
            return Ok(result);
        }

        /// <summary>
        /// get delivery order by housebill id
        /// </summary>
        /// <param name="hblid"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpGet("GetDeliveryOrder")]
        //[Authorize]
        public IActionResult GetDeliveryOrder(Guid hblid, TransactionTypeEnum type)
        {
            string transactionType = DataTypeEx.GetType(type);
            var result = arrivalFreightChargeServices.GetDeliveryOrder(hblid, transactionType);
            return Ok(result);
        }

        /// <summary>
        /// update arrival info
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut("UpdateArrival")]
        [Authorize]
        public IActionResult UpdateArrival(CsArrivalViewModel model)
        {
            var hs = arrivalFreightChargeServices.UpdateArrival(model);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// update delivery order
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut("UpdateDeliveryOrder")]
        [Authorize]
        public IActionResult UpdateDeliveryOrder(DeliveryOrderViewModel model)
        {
            var hs = arrivalFreightChargeServices.UpdateDeliveryOrder(model);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// set freight charge default of an user in a shipment type
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut("SetArrivalChargeDefault")]
        [Authorize]
        public IActionResult SetArrivalChargeDefault(CsArrivalFrieghtChargeDefaultEditModel model)
        {
            var hs = arrivalFreightChargeServices.SetArrivalChargeDefault(model);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// set header - footer default of arrival
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut("SetArrivalHeaderFooterDefault")]
        [Authorize]
        public IActionResult SetArrivalHeaderFooterDefault(CsArrivalDefaultModel model)
        {
            var hs = arrivalFreightChargeServices.SetArrivalHeaderFooterDefault(model);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// set header - footer default of delivery order
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut("SetDeliveryOrderHeaderFooterDefault")]
        [Authorize]
        public IActionResult SetDeliveryOrderHeaderFooterDefault(CsDeliveryOrderDefaultModel model)
        {
            var hs = arrivalFreightChargeServices.SetDeliveryOrderHeaderFooterDefault(model);
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// delete an existed charge
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("DeleteCharge")]
        [Authorize]
        public IActionResult DeleteCharge(Guid id)
        {
            var hs = arrivalFreightChargeServices.Delete(x => x.Id == id);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// return data source of deliver order report
        /// </summary>
        /// <returns></returns>
        [HttpGet("PreviewDeliveryOrder")]
        [Authorize]
        public IActionResult PreviewDeliveryOrder(Guid hblid)
        {
            var data = arrivalFreightChargeServices.PreviewDeliveryOrder(hblid);
            return Ok(data);
        }

        /// <summary>
        /// return data source of arrival notice report (Sea)
        /// </summary>
        /// <returns></returns>
        [HttpPost("PreviewArrivalNotice")]
        public IActionResult PreviewArrivalNotice(PreviewArrivalNoticeCriteria criteria)
        {
            var data = arrivalFreightChargeServices.PreviewArrivalNoticeSIF(criteria);
            return Ok(data);
        }

        /// <summary>
        /// return data source of arrival notice report (Air)
        /// </summary>
        /// <returns></returns>
        [HttpPost("PreviewArrivalNoticeAir")]
        public IActionResult PreviewArrivalNoticeAir(PreviewArrivalNoticeCriteria criteria)
        {
            var data = arrivalFreightChargeServices.PreviewArrivalNoticeAir(criteria);
            return Ok(data);
        }
    }
}
