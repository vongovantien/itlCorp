﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.NoSql;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Shipment.Infrastructure.Common;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
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
        /// get and paging the list of Advance Payment by conditions
        /// </summary>
        /// <param name="criteria">search conditions</param>
        /// <param name="pageNumber">page to retrieve data</param>
        /// <param name="pageSize">number items per page</param>
        /// <returns></returns>
        [HttpPost]
        [Route("Paging")]
        public IActionResult Paging(AcctAdvancePaymentCriteria criteria, int pageNumber, int pageSize)
        {
            var data = acctAdvancePaymentService.Paging(criteria, pageNumber, pageSize, out int totalItems);
            var result = new { data, totalItems, pageNumber, pageSize };
            return Ok(result);
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
        [Authorize]
        public IActionResult Add(AcctAdvancePaymentModel model)
        {
            if (!ModelState.IsValid) return BadRequest();

            if(model.AdvanceRequests.Count > 0)
            {
                //Nếu sum(Amount) > 100.000.000 & Payment Method là Cash thì báo lỗi
                if (model.PaymentMethod.Equals("Cash"))
                {
                    var totalAmount = model.AdvanceRequests.Sum(x => x.Amount);
                    if (totalAmount > 100000000)
                    {
                        ResultHandle _result = new ResultHandle { Status = false, Message = "Total Advance Amount by cash is not exceed 100.000.000 VND" };
                        return BadRequest(_result);
                    }
                }

                //Kiểm tra tồn tại shipment trong 1 Advance Payment khác. Nếu đã tồn tại thì báo lỗi
                foreach(var item in model.AdvanceRequests)
                {
                    var shipment = new ShipmentAdvancePaymentCriteria
                    {
                        JobId = item.JobId,
                        HBL = item.Hbl,
                        MBL = item.Mbl,
                        AdvanceNo = item.AdvanceNo
                    };
                    if (acctAdvancePaymentService.CheckShipmentsExistInAdvancePayment(shipment))
                    {
                        ResultHandle _result = new ResultHandle { Status = false, Message = "Duplicate Shipment" };
                        return BadRequest(_result);
                    }
                }
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
            var data = acctAdvancePaymentService.CheckShipmentsExistInAdvancePayment(criteria);
            ResultHandle result = new ResultHandle { Status = data, Message = data ? "Exists" : "Not exists" };
            return Ok(result);
        }

        /// <summary>
        /// delete an existed item
        /// </summary>
        /// <param name="idAdvanceRequest">idAdvanceRequest of existed item that want to delete</param>
        /// <returns></returns>
        [Authorize]
        [HttpDelete]
        [Route("Delete")]
        public IActionResult Delete(Guid idAdvanceRequest)
        {
            ChangeTrackerHelper.currentUser = currentUser.UserID;

            HandleState hs = acctAdvancePaymentService.DeleteAdvanceRequest(idAdvanceRequest);

            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Get Advance Payment by AdvanceNo
        /// </summary>
        /// <param name="advanceNo">advanceNo that want to retrieve Advance Payment</param>
        /// <returns></returns>
        //[HttpGet]
        //[Route("GetAdvancePaymentByAdvanceNo")]
        //public IActionResult GetAdvancePaymentByAdvanceNo(string advanceNo)
        //{
        //    var data = acctAdvancePaymentService.GetAdvancePaymentByAdvanceNo(advanceNo);
        //    if(data != null)
        //        return Ok(data);
        //    return NotFound();
        //}

    }
}