using System;
using System.Linq;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Accounting.DL.Common;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.Criteria;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using eFMS.API.Accounting.Infrastructure.Middlewares;
using System.Collections.Generic;
using eFMS.API.Common.Infrastructure.Common;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using eFMS.API.Common.Helpers;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using System.Globalization;
using System.Text.RegularExpressions;

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
        private readonly IHostingEnvironment _hostingEnvironment;


        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="service"></param>
        /// <param name="user"></param>
        /// <param name="hostingEnvironment"></param>
        public AcctAdvancePaymentController(IStringLocalizer<LanguageSub> localizer, IAcctAdvancePaymentService service, ICurrentUser user, IHostingEnvironment hostingEnvironment)
        {
            stringLocalizer = localizer;
            acctAdvancePaymentService = service;
            currentUser = user;
            _hostingEnvironment = hostingEnvironment;

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
        [Authorize]
        public IActionResult Paging(AcctAdvancePaymentCriteria criteria, int pageNumber, int pageSize)
        {
            var data = acctAdvancePaymentService.Paging(criteria, pageNumber, pageSize, out int totalItems);
            var result = new { data, totalItems, pageNumber, pageSize };
            return Ok(result);
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

        [HttpPost]
        [Route("GetGroupRequestsByAdvanceNoList")]
        public IActionResult GetGrpRequestsByAdvanceNo(string[] advanceNoList)
        {
            //Sum(Amount) theo lô hàng (JobId, HBL)
            var data = acctAdvancePaymentService.GetGroupRequestsByAdvanceNoList(advanceNoList);
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

            if (model.AdvanceRequests.Count > 0)
            {
                //Nếu sum(Amount) > 100.000.000 & Payment Method là Cash thì báo lỗi
                if (model.PaymentMethod.Equals(AccountingConstants.PAYMENT_METHOD_CASH))
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
            if (hs.Code == 403)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.DO_NOT_HAVE_PERMISSION].Value });
            }

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
        /// Check allow delete advance payment
        /// </summary>
        /// <param name="id">Id of advance payment</param>
        /// <returns></returns>
        [HttpGet("CheckAllowDelete/{id}")]
        [Authorize]
        public IActionResult CheckAllowDelete(Guid id)
        {
            var result = acctAdvancePaymentService.CheckDeletePermissionByAdvanceId(id);
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
            var isAllowDelete = acctAdvancePaymentService.CheckDeletePermissionByAdvanceNo(advanceNo);
            if (isAllowDelete == false)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.DO_NOT_HAVE_PERMISSION].Value });
            }

            HandleState hs = acctAdvancePaymentService.DeleteAdvancePayment(advanceNo);
            if (hs.Code == 403)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.DO_NOT_HAVE_PERMISSION].Value });
            }

            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Check allow detail advance payment
        /// </summary>
        /// <param name="id">Id of advance payment</param>
        /// <returns></returns>
        [HttpGet("CheckAllowDetail/{id}")]
        [Authorize]
        public IActionResult CheckAllowDetail(Guid id)
        {
            var result = acctAdvancePaymentService.CheckDetailPermissionByAdvanceId(id);
            return Ok(result);
        }

        /// <summary>
        /// Get Advance Payment by AdvanceNo
        /// </summary>
        /// <param name="advanceNo">advanceNo that want to retrieve Advance Payment</param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetAdvancePaymentByAdvanceNo")]
        [Authorize]
        public IActionResult GetAdvancePaymentByAdvanceNo(string advanceNo)
        {
            var isAllowViewDetail = acctAdvancePaymentService.CheckDetailPermissionByAdvanceNo(advanceNo);
            if (isAllowViewDetail == false)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.DO_NOT_HAVE_PERMISSION].Value });
            }

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
        [Authorize]
        public IActionResult GetAdvancePaymentByAdvanceId(Guid advanceId)
        {
            var isAllowViewDetail = acctAdvancePaymentService.CheckDetailPermissionByAdvanceId(advanceId);
            if (isAllowViewDetail == false)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.DO_NOT_HAVE_PERMISSION].Value });
            }

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

            var isAllowUpdate = acctAdvancePaymentService.CheckUpdatePermissionByAdvanceId(model.Id);
            if (isAllowUpdate == false)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.DO_NOT_HAVE_PERMISSION].Value });
            }

            //Đã check bên trong function UpdateAdvancePayment
            //if (!model.StatusApproval.Equals(Constants.STATUS_APPROVAL_NEW) && !model.StatusApproval.Equals(Constants.STATUS_APPROVAL_DENIED))
            //{
            //    ResultHandle _result = new ResultHandle { Status = false, Message = "Only allowed to edit the advance payment status is New or Deny" };
            //    return BadRequest(_result);
            //}

            if (model.AdvanceRequests.Count > 0)
            {
                //Nếu sum(Amount) > 100.000.000 & Payment Method là Cash thì báo lỗi
                if (model.PaymentMethod.Equals(AccountingConstants.PAYMENT_METHOD_CASH))
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
            if (hs.Code == 403)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.DO_NOT_HAVE_PERMISSION].Value });
            }

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
        [Authorize]
        public IActionResult SaveAndSendRequest(AcctAdvancePaymentModel model)
        {
            if (!ModelState.IsValid) return BadRequest();

            if (model.AdvanceRequests.Count > 0)
            {
                //Nếu sum(Amount) > 100.000.000 & Payment Method là Cash thì báo lỗi
                if (model.PaymentMethod.Equals(AccountingConstants.PAYMENT_METHOD_CASH))
                {
                    var totalAmount = model.AdvanceRequests.Select(s => s.Amount).Sum();
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
            var message = string.Empty;
            if (string.IsNullOrEmpty(model.AdvanceNo))//Insert Advance Payment
            {
                #region -- Check Exist Setting Flow --
                var isExistSettingFlow = acctAdvancePaymentService.CheckExistSettingFlow("Advance", currentUser.OfficeID);
                if (!isExistSettingFlow.Success)
                {
                    ResultHandle _result = new ResultHandle { Status = false, Message = isExistSettingFlow.Exception.Message };
                    return BadRequest(_result);
                }
                #endregion -- Check Exist Setting Flow --

                #region -- Check Exist User Approval --
                var isExistUserApproval = acctAdvancePaymentService.CheckExistUserApproval("Advance", currentUser.GroupId, currentUser.DepartmentId, currentUser.OfficeID, currentUser.CompanyID);
                if (!isExistUserApproval.Success)
                {
                    ResultHandle _result = new ResultHandle { Status = false, Message = isExistUserApproval.Exception.Message };
                    return BadRequest(_result);
                }
                #endregion -- Check Exist User Approval --

                model.StatusApproval = AccountingConstants.STATUS_APPROVAL_REQUESTAPPROVAL;
                hs = acctAdvancePaymentService.AddAdvancePayment(model);
                message = HandleError.GetMessage(hs, Crud.Insert);
                if (hs.Code == 403)
                {
                    return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.DO_NOT_HAVE_PERMISSION].Value });
                }
            }
            else //Update Advance Payment
            {
                var isAllowUpdate = acctAdvancePaymentService.CheckUpdatePermissionByAdvanceId(model.Id);
                if (isAllowUpdate == false)
                {
                    return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.DO_NOT_HAVE_PERMISSION].Value });
                }

                var advancePaymentCurrent = acctAdvancePaymentService.Get(x => x.Id == model.Id).FirstOrDefault();
                #region -- Check Exist Advance Payment --
                if (advancePaymentCurrent == null)
                {
                    ResultHandle _result = new ResultHandle { Status = false, Message = "Not found advance payment" };
                    return BadRequest(_result);
                }
                #endregion -- Check Exist Advance Payment --

                #region -- Check Exist Setting Flow --
                var isExistSettingFlow = acctAdvancePaymentService.CheckExistSettingFlow("Advance", advancePaymentCurrent.OfficeId);
                if (!isExistSettingFlow.Success)
                {
                    ResultHandle _result = new ResultHandle { Status = false, Message = isExistSettingFlow.Exception.Message };
                    return BadRequest(_result);
                }
                #endregion -- Check Exist Setting Flow --

                #region -- Check Exist User Approval --
                var isExistUserApproval = acctAdvancePaymentService.CheckExistUserApproval("Advance", advancePaymentCurrent.GroupId, advancePaymentCurrent.DepartmentId, advancePaymentCurrent.OfficeId, advancePaymentCurrent.CompanyId);
                if (!isExistUserApproval.Success)
                {
                    ResultHandle _result = new ResultHandle { Status = false, Message = isExistUserApproval.Exception.Message };
                    return BadRequest(_result);
                }
                #endregion -- Check Exist User Approval --

                #region -- Check Advance Payment Approving --
                if (!model.StatusApproval.Equals(AccountingConstants.STATUS_APPROVAL_NEW) && !model.StatusApproval.Equals(AccountingConstants.STATUS_APPROVAL_DENIED))
                {
                    ResultHandle _result = new ResultHandle { Status = false, Message = "Only allowed to edit the advance payment status is New or Deny" };
                    return BadRequest(_result);
                }
                #endregion -- Check Advance Payment Approving --

                model.StatusApproval = AccountingConstants.STATUS_APPROVAL_REQUESTAPPROVAL;
                hs = acctAdvancePaymentService.UpdateAdvancePayment(model);
                message = HandleError.GetMessage(hs, Crud.Update);
                if (hs.Code == 403)
                {
                    return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.DO_NOT_HAVE_PERMISSION].Value });
                }
            }

            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = model };
            if (hs.Success)
            {
                AcctApproveAdvanceModel approve = new AcctApproveAdvanceModel
                {
                    AdvanceNo = model.AdvanceNo,
                    Requester = model.Requester,
                    RequesterAprDate = DateTime.Now
                };
                var resultInsertUpdateApprove = acctAdvancePaymentService.InsertOrUpdateApprovalAdvance(approve);
                if (!resultInsertUpdateApprove.Success)
                {
                    ResultHandle _result = new ResultHandle { Status = false, Message = resultInsertUpdateApprove.Exception.Message };
                    return BadRequest(_result);
                }
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
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
                return BadRequest(_result);
            }
            else
            {
                _result = new ResultHandle { Status = updateApproval.Success };
                return Ok(_result);
            }
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
                _result = new ResultHandle { Status = denieApproval.Success, Message = denieApproval.Exception.Message };
                return BadRequest(_result);
            }
            else
            {
                _result = new ResultHandle { Status = denieApproval.Success };
                return Ok(_result);
            }
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

        /// <summary>
        /// Get list advance of shipment
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetAdvancesOfShipment")]
        public IActionResult GetAdvancesOfShipment(string jobId)
        {
            var data = acctAdvancePaymentService.GetAdvancesOfShipment().Where(x => x.JobId == jobId).OrderByDescending(o => o.RequestDate);
            return Ok(data);
        }

        /// <summary>
        /// Get advances to unlock
        /// </summary>
        /// <param name="keyWords"></param>
        /// <returns></returns>
        [HttpPost("GetAdvancesToUnlock")]
        public IActionResult GetAdvancePayment(List<string> keyWords)
        {
            if (keyWords == null) return Ok(new LockedLogResultModel());
            LockedLogResultModel result = acctAdvancePaymentService.GetAdvanceToUnlock(keyWords);
            return Ok(result);
        }

        /// <summary>
        /// Unlock advance
        /// </summary>
        /// <param name="advancePayments"></param>
        /// <returns></returns>
        [HttpPost("UnLock")]
        [Authorize]
        public IActionResult UnLock(List<LockedLogModel> advancePayments)
        {
            var result = acctAdvancePaymentService.UnLock(advancePayments);
            return Ok(result);
        }

        /// <summary>
        /// Export advance payment by advance id
        /// </summary>
        /// <param name="advanceId">Id of advance payment</param>
        /// <param name="language"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("DetailAdvancePaymentExport")]
        public IActionResult DetailAdvancePaymentExport(Guid advanceId, string language)
        {
            var result = acctAdvancePaymentService.AdvancePaymentExport(advanceId, language);
            return Ok(result);
        }

        /// <summary>
        /// Recall Request Advance 
        /// </summary>
        /// <param name="advanceId">advanceId that want to retrieve Update Approve</param>
        /// <returns></returns>
        [HttpPost]
        [Route("RecallRequest")]
        public IActionResult RecallRequest(Guid advanceId)
        {
            var updateApproval = acctAdvancePaymentService.RecallRequest(advanceId);
            ResultHandle _result;
            if (!updateApproval.Success)
            {
                _result = new ResultHandle { Status = updateApproval.Success, Message = updateApproval.Exception.Message };
                return BadRequest(_result);
            }
            else
            {
                _result = new ResultHandle { Status = updateApproval.Success };
                return Ok(_result);
            }
        }

        /// <summary>
        /// Update Approve Advance
        /// </summary>
        /// <param name="model">advanceIds that want to retrieve Update Approve</param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdatePaymentVoucher")]
        public IActionResult UpdatePaymentVoucher(AcctAdvancePaymentModel model)
        {
            HandleState updatePayment = acctAdvancePaymentService.UpdatePaymentVoucher(model);
            ResultHandle result;
            if (!updatePayment.Success)
            {
                result = new ResultHandle { Status = updatePayment.Success, Message = updatePayment.Exception.Message };
                return BadRequest(result);
            }
            else
            {
                string message = HandleError.GetMessage(updatePayment, Crud.Update);
                result = new ResultHandle { Status = updatePayment.Success, Message = stringLocalizer[message].Value };
                return Ok(result);
            }
        }

        /// <summary>
        /// Get advances to unlock
        /// </summary>
        /// <param name="lstVoucher"></param>
        /// <returns></returns>
        [HttpPost("CheckExistedVoucherInAdvance")]
        public IActionResult CheckExistedVoucherInAdvance(List<AccAdvancePaymentUpdateVoucher> lstVoucher)
        {
            List<AccAdvancePaymentUpdateVoucher> lstVoucherData = new List<AccAdvancePaymentUpdateVoucher>();

            foreach (var item in lstVoucher)
            {
                if (acctAdvancePaymentService.Any(x => x.Id == item.Id && x.VoucherNo != null))
                {
                    lstVoucherData.Add(item);
                }
            }
            var result = new { lstVoucherData };
            return Ok(result);
        }

        [HttpPost]
        [Route("import")]
        [Authorize]
        public IActionResult Import([FromBody] List<AccAdvancePaymentVoucherImportModel> data)
        {
            var hs = acctAdvancePaymentService.Import(data);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = "Import successfully !!!" };
            if (!hs.Success)
            {
                return BadRequest(new ResultHandle { Status = false, Message = hs.Message.ToString() });
            }
            return Ok(result);
        }

        /// <summary>
        /// download file excel from server
        /// </summary>
        /// <returns></returns>
        [HttpGet("DownloadExcel")]
        public async Task<ActionResult> DownloadExcel()
        {
            string fileName = Templates.AccAdvance.ExelImportFileName + Templates.ExelImportEx;
            string templateName = _hostingEnvironment.ContentRootPath;
            var result = await new FileHelper().ExportExcel(templateName, fileName);
            if (result != null)
            {
                return result;
            }
            else
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.FILE_NOT_FOUND].Value });
            }
        }

        /// <summary>
        /// read data from file excel
        /// </summary>
        /// <param name="uploadedFile"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("uploadFile")]
        public IActionResult UploadFile(IFormFile uploadedFile)
        {
            var file = new FileHelper().UploadExcel(uploadedFile);
            if (file != null)
            {
                bool ValidDate = false;
                DateTime temp;

                ExcelWorksheet worksheet = file.Workbook.Worksheets[1];
                int rowCount = worksheet.Dimension.Rows;
                int ColCount = worksheet.Dimension.Columns;
                if (rowCount < 2) return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.NOT_FOUND_DATA_EXCEL].Value });
                List<AccAdvancePaymentVoucherImportModel> list = new List<AccAdvancePaymentVoucherImportModel>();
                for (int row = 2; row <= rowCount; row++)
                {
                    string date = worksheet.Cells[row, 3].Value?.ToString().Trim();
                    DateTime? dateToPase = null;
                    if (DateTime.TryParse(date, out temp))
                    {
                        CultureInfo culture = new CultureInfo("es-ES");
                        dateToPase = DateTime.Parse(date, culture);
                    }
                    else
                    {
                        CultureInfo culture = new CultureInfo("es-ES");
                        dateToPase = DateTime.Parse(date, culture);
                    }
                    var acc = new AccAdvancePaymentVoucherImportModel
                    {
                        IsValid = true,
                        AdvanceNo = worksheet.Cells[row, 1].Value?.ToString().Trim(),
                        VoucherNo = worksheet.Cells[row, 2].Value?.ToString().Trim(),
                        VoucherDate = !string.IsNullOrEmpty(date) ? dateToPase : (DateTime?)null,

                    };
                    list.Add(acc);
                }
                var data = acctAdvancePaymentService.CheckValidImport(list, ValidDate);
                var totalValidRows = data.Count(x => x.IsValid == true);
                var results = new { data, totalValidRows };
                return Ok(results);
            }
            return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.FILE_NOT_FOUND].Value });
        }

        [HttpGet]
        [Route("GetHistoryDeniedAdvancePayment")]
        public IActionResult GetHistoryDeniedAdvance(string advanceNo)
        {
            var data = acctAdvancePaymentService.GetHistoryDeniedAdvance(advanceNo);
            return Ok(data);
        }
    }
}