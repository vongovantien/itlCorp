using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.NoSql;
using eFMS.API.Accounting.DL.Common;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.Criteria;
using eFMS.API.Accounting.Infrastructure.Common;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using eFMS.API.Accounting.Infrastructure.Middlewares;
namespace eFMS.API.Accounting.Controllers
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
        /// get list advance payment by conditions
        /// </summary>
        /// <param name="criteria">search conditions</param>
        /// <returns></returns>
        [HttpPost]
        [Route("QueryData")]
        public IActionResult QueryData(AcctAdvancePaymentCriteria criteria)
        {
            var data = acctAdvancePaymentService.QueryData(criteria);
            return Ok(data);
        }

        /// <summary>
        /// Get Group Requests by AdvanceNo
        /// </summary>
        /// <param name="advanceNo">advanceNo that want to retrieve Advance Request</param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetGroupRequestsByAdvanceNo")]
        public IActionResult GetGrpRequestsByAdvanceNo(string advanceNo)
        {
            //Sum(Amount) theo lô hàng (JobId, HBL)
            var data = acctAdvancePaymentService.GetGroupRequestsByAdvanceNo(advanceNo);
            return Ok(data);
        }

        /// <summary>
        /// Get Group Requests by AdvanceId
        /// </summary>
        /// <param name="advanceId">advanceId that want to retrieve Advance Request</param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetGroupRequestsByAdvanceId")]
        public IActionResult GetGrpRequestsByAdvanceId(Guid advanceId)
        {
            //Sum(Amount) theo lô hàng (JobId, HBL)
            var data = acctAdvancePaymentService.GetGroupRequestsByAdvanceId(advanceId);
            return Ok(data);
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
                if (model.PaymentMethod.Equals(Constants.PAYMENT_METHOD_CASH))
                {
                    var totalAmount = model.AdvanceRequests.Sum(x => x.Amount);
                    if (totalAmount > 100000000)
                    {
                        ResultHandle _result = new ResultHandle { Status = false, Message = "Total Advance Amount by cash is not exceed 100.000.000 VND" };
                        return BadRequest(_result);
                    }
                }

                //Kiểm tra tồn tại shipment trong 1 Advance Payment khác. Nếu đã tồn tại thì báo lỗi 
                //Updated 27/08/2019 by Andy.Hoa - [ĐÃ THAY ĐỔI YÊU CẦU - 1 SHIPMENT CHO PHÉP ĐƯỢC TẠO TRONG NHIỀU ADVANCE PAYMENT]
                //foreach(var item in model.AdvanceRequests)
                //{
                //    var shipment = new ShipmentAdvancePaymentCriteria
                //    {
                //        JobId = item.JobId,
                //        HBL = item.Hbl,
                //        MBL = item.Mbl,
                //        AdvanceNo = item.AdvanceNo
                //    };
                //    if (acctAdvancePaymentService.CheckShipmentsExistInAdvancePayment(shipment))
                //    {
                //        ResultHandle _result = new ResultHandle { Status = false, Message = "Duplicate Shipment" };
                //        return BadRequest(_result);
                //    }
                //}
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
        /// delete an advance payment existed item
        /// </summary>
        /// <param name="advanceNo">advanceNo of existed item that want to delete</param>
        /// <returns></returns>
        [Authorize]
        [HttpDelete]
        [Route("Delete")]
        public IActionResult Delete(string advanceNo)
        {
            ChangeTrackerHelper.currentUser = currentUser.UserID;

            HandleState hs = acctAdvancePaymentService.DeleteAdvancePayment(advanceNo);

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
        [HttpGet]
        [Route("GetAdvancePaymentByAdvanceNo")]
        public IActionResult GetAdvancePaymentByAdvanceNo(string advanceNo)
        {
            var data = acctAdvancePaymentService.GetAdvancePaymentByAdvanceNo(advanceNo);
            return Ok(data);
        }

        /// <summary>
        /// Get Advance Payment by AdvanceId
        /// </summary>
        /// <param name="advanceId">advanceId that want to retrieve Advance Payment</param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetAdvancePaymentByAdvanceId")]
        public IActionResult GetAdvancePaymentByAdvanceId(Guid advanceId)
        {
            var data = acctAdvancePaymentService.GetAdvancePaymentByAdvanceId(advanceId);
            return Ok(data);
        }

        /// <summary>
        /// Update Advance Payment
        /// </summary>
        /// <param name="model">object to update</param>
        /// <returns></returns>
        [HttpPut]
        [Route("Update")]
        [Authorize]
        public IActionResult Update(AcctAdvancePaymentModel model)
        {
            if (!ModelState.IsValid) return BadRequest();

            if (!model.StatusApproval.Equals(Constants.STATUS_APPROVAL_NEW) && !model.StatusApproval.Equals(Constants.STATUS_APPROVAL_DENIED))
            {
                ResultHandle _result = new ResultHandle { Status = false, Message = "Only allowed to edit the advance payment status is New or Deny" };
                return BadRequest(_result);
            }

            if (model.AdvanceRequests.Count > 0)
            {
                //Nếu sum(Amount) > 100.000.000 & Payment Method là Cash thì báo lỗi
                if (model.PaymentMethod.Equals(Constants.PAYMENT_METHOD_CASH))
                {
                    var totalAmount = model.AdvanceRequests.Sum(x => x.Amount);
                    if (totalAmount > 100000000)
                    {
                        ResultHandle _result = new ResultHandle { Status = false, Message = "Total Advance Amount by cash is not exceed 100.000.000 VND" };
                        return BadRequest(_result);
                    }
                }

                //Kiểm tra tồn tại shipment trong 1 Advance Payment khác. Nếu đã tồn tại thì báo lỗi
                //Updated 27/08/2019 by Andy.Hoa - [ĐÃ THAY ĐỔI YÊU CẦU - 1 SHIPMENT CHO PHÉP ĐƯỢC TẠO TRONG NHIỀU ADVANCE PAYMENT]
                //foreach (var item in model.AdvanceRequests)
                //{
                //    var shipment = new ShipmentAdvancePaymentCriteria
                //    {
                //        JobId = item.JobId,
                //        HBL = item.Hbl,
                //        MBL = item.Mbl,
                //        AdvanceNo = model.AdvanceNo//Truyền vào Advance No cần update
                //    };
                //    if (acctAdvancePaymentService.CheckShipmentsExistInAdvancePayment(shipment))
                //    {
                //        ResultHandle _result = new ResultHandle { Status = false, Message = "Duplicate Shipment" };
                //        return BadRequest(_result);
                //    }
                //}
            }

            var hs = acctAdvancePaymentService.UpdateAdvancePayment(model);

            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = model };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Preview Advance Payment Request By Advance Id
        /// </summary>
        /// <param name="advanceId">advanceId that want to retrieve preview advance payment request</param>
        /// <returns></returns>
        [HttpPost]
        [Route("PreviewAdvancePaymentRequestByAdvanceId")]
        public IActionResult PreviewAdvancePaymentRequest(Guid advanceId)
        {
            var result = acctAdvancePaymentService.Preview(advanceId);
            return Ok(result);
        }

        /// <summary>
        /// Preview Advance Payment Request
        /// </summary>
        /// <param name="advance"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("PreviewAdvancePaymentRequest")]
        public IActionResult PreviewAdvancePaymentRequest(AcctAdvancePaymentModel advance)
        {
            var result = acctAdvancePaymentService.Preview(advance);
            return Ok(result);
        }

        /// <summary>
        /// Save and Send Request
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("SaveAndSendRequest")]
        public IActionResult SaveAndSendRequest(AcctAdvancePaymentModel model)
        {
            if (!ModelState.IsValid) return BadRequest();           

            if (model.AdvanceRequests.Count > 0)
            {
                //Nếu sum(Amount) > 100.000.000 & Payment Method là Cash thì báo lỗi
                if (model.PaymentMethod.Equals(Constants.PAYMENT_METHOD_CASH))
                {
                    var totalAmount = model.AdvanceRequests.Sum(x => x.Amount);
                    if (totalAmount > 100000000)
                    {
                        ResultHandle _result = new ResultHandle { Status = false, Message = "Total Advance Amount by cash is not exceed 100.000.000 VND" };
                        return BadRequest(_result);
                    }
                }

                //Kiểm tra tồn tại shipment trong 1 Advance Payment khác. Nếu đã tồn tại thì báo lỗi
                //Updated 27/08/2019 by Andy.Hoa - [ĐÃ THAY ĐỔI YÊU CẦU - 1 SHIPMENT CHO PHÉP ĐƯỢC TẠO TRONG NHIỀU ADVANCE PAYMENT]
                //foreach (var item in model.AdvanceRequests)
                //{
                //    var shipment = new ShipmentAdvancePaymentCriteria
                //    {
                //        JobId = item.JobId,
                //        HBL = item.Hbl,
                //        MBL = item.Mbl,
                //        AdvanceNo = model.AdvanceNo//Truyền vào Advance No cần update
                //    };
                //    if (acctAdvancePaymentService.CheckShipmentsExistInAdvancePayment(shipment))
                //    {
                //        ResultHandle _result = new ResultHandle { Status = false, Message = "Duplicate Shipment" };
                //        return BadRequest(_result);
                //    }
                //}
            }           

            HandleState hs;
            if (string.IsNullOrEmpty(model.AdvanceNo))//Insert Advance Payment
            {
                //Change request: Bỏ status RequestApproval
                //model.StatusApproval = "RequestApproval";
                hs = acctAdvancePaymentService.AddAdvancePayment(model);
            }
            else //Update Advance Payment
            {
                if (!model.StatusApproval.Equals(Constants.STATUS_APPROVAL_NEW) && !model.StatusApproval.Equals(Constants.STATUS_APPROVAL_DENIED))
                {
                    ResultHandle _result = new ResultHandle { Status = false, Message = "Only allowed to edit the advance payment status is New or Deny" };
                    return BadRequest(_result);
                }
                //Change request: Bỏ status RequestApproval
                //model.StatusApproval = "RequestApproval";
                hs = acctAdvancePaymentService.UpdateAdvancePayment(model);
            }

            AcctApproveAdvanceModel approve = new AcctApproveAdvanceModel
            {
                AdvanceNo = model.AdvanceNo,
                Requester = model.Requester
            };
            var resultInsertUpdateApprove = acctAdvancePaymentService.InsertOrUpdateApprovalAdvance(approve);
            if (!resultInsertUpdateApprove.Success)
            {
                ResultHandle _result = new ResultHandle { Status = false, Message = resultInsertUpdateApprove.Exception.Message };
                return BadRequest(_result);
            }

            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = model };
            if (!hs.Success)
            {
                return Ok(result);
            }
            return Ok(result);           
        }

        /// <summary>
        /// Update Approve Advance
        /// </summary>
        /// <param name="advanceId">advanceId that want to retrieve Update Approve</param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateApprove")]
        public IActionResult UpdateApprove(Guid advanceId)
        {
            var updateApproval = acctAdvancePaymentService.UpdateApproval(advanceId);
            ResultHandle _result;
            if (!updateApproval.Success)
            {
                _result = new ResultHandle { Status = updateApproval.Success, Message = updateApproval.Exception.Message };
            }
            else
            {
                _result = new ResultHandle { Status = updateApproval.Success };
            }
            return Ok(_result);
        }

        /// <summary>
        /// Denie Approve Advance
        /// </summary>
        /// <param name="advanceId">advanceId that want to retrieve Denie Approve</param>
        /// <param name="comment">comment reason that want to retrieve Denie Approve</param>
        /// <returns></returns>
        [HttpPost]
        [Route("DeniedApprove")]
        public IActionResult DeniedApprove(Guid advanceId, string comment)
        {
            var denieApproval = acctAdvancePaymentService.DeniedApprove(advanceId, comment);
            ResultHandle _result;
            if (!denieApproval.Success)
            {
                _result  = new ResultHandle { Status = denieApproval.Success, Message = denieApproval.Exception.Message };
            }
            else
            {
                _result = new ResultHandle { Status = denieApproval.Success };               
            }
            return Ok(_result);
        }

        /// <summary>
        /// Get information approve advance by advanceNo
        /// </summary>
        /// <param name="advanceNo">advanceNo that want to retrieve approve advance</param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetInfoApproveAdvanceByAdvanceNo")]
        public IActionResult GetInfoApproveAdvanceByAdvanceNo(string advanceNo)
        {
            var data = acctAdvancePaymentService.GetInfoApproveAdvanceByAdvanceNo(advanceNo);
            return Ok(data);
        }
    }
}