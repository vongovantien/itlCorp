using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Shipment.Infrastructure.Common;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
    public class AcctAdvancePaymentController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly IAcctAdvancePaymentService acctAdvancePaymentService;
        private readonly ICurrentUser currentUser;

        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="service"></param>
        /// <param name="user"></param>
        public AcctAdvancePaymentController(IStringLocalizer<LanguageSub> localizer, IAcctAdvancePaymentService service, ICurrentUser user)
        {
            stringLocalizer = localizer;
            acctAdvancePaymentService = service;
            currentUser = user;
        }

        /// <summary>
        /// Get shipments (JobId, HBL, MBL) from shipment documentation and shipment operation
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetShipments")]
        public IActionResult GetShipments()
        {
            var data = acctAdvancePaymentService.GetShipments();
            return Ok(data);
        }

        /// <summary>
        /// add new advance payment (include advance request)
        /// </summary>
        /// <param name="model">object to add</param>
        /// <returns></returns>
        [HttpPost]
        [Route("Add")]
        //[Authorize]
        public IActionResult Add(AcctAdvancePaymentModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            if(model.AdvanceRequests.Count > 0)
            {
                var totalAmount = model.AdvanceRequests;
            }

            var hs = acctAdvancePaymentService.AddAdvancePayment(model);

            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = model };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Check shipment exists in advance payment
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("CheckShipmentsExistInAdvancePament")]
        public IActionResult CheckShipmentsExistInAdvancePayment(ShipmentAdvancePaymentCriteria criteria)
        {
            var result = acctAdvancePaymentService.CheckShipmentsExistInAdvancePayment(criteria);
            return Ok(result);
        }
    }
}