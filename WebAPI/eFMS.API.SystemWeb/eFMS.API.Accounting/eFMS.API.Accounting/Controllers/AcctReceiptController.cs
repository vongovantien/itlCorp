using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.Criteria;
using eFMS.API.Accounting.DL.Common;
using eFMS.API.Accounting.DL.Models.Receipt;
using eFMS.API.Accounting.Infrastructure.Middlewares;
using eFMS.API.Accounting.Service.Models;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Models;
using eFMS.API.Common.Infrastructure.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using eFMS.IdentityServer.DL.UserManager;
using eFMS.API.Infrastructure.Extensions;
using ITL.NetCore.Common;
using System.Threading.Tasks;
using System.Net.Http;
using eFMS.API.Common.Helpers;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using eFMS.API.Infrastructure.RabbitMQ;

namespace eFMS.API.Accounting.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class AcctReceiptController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly IAcctReceiptService acctReceiptService;
        private readonly ICurrentUser currentUser;
        private readonly IOptions<ApiUrl> apiServiceUrl;
        private readonly IRabbitBus _busControl; 

        public AcctReceiptController(IStringLocalizer<LanguageSub> localizer,
            ICurrentUser curUser,
            IOptions<ApiUrl> _apiServiceUrl,
            IRabbitBus _bus,
           IAcctReceiptService acctReceipt)
        {
            stringLocalizer = localizer;
            acctReceiptService = acctReceipt;
            currentUser = curUser;
            apiServiceUrl = _apiServiceUrl;
            _busControl = _bus;
        }

        [HttpPost]
        [Route("Query")]
        public IActionResult Query(AcctReceiptCriteria criteria)
        {
            IQueryable<AcctReceipt> result = acctReceiptService.Query(criteria);

            return Ok(result);
        }

        [HttpPost]
        [Route("Paging")]
        [Authorize]
        public IActionResult Paging(AcctReceiptCriteria criteria, int page, int size)
        {
            IQueryable<AcctReceiptModel> data = acctReceiptService.Paging(criteria, page, size, out int rowsCount);
            var result = new ResponsePagingModel<AcctReceiptModel> { Data = data, Page = page, Size = size, TotalItems = rowsCount };
            return Ok(result);
        }


        //[HttpGet("GenerateReceiptNo")]
        //[Authorize]
        //public IActionResult GenerateReceiptNo()
        //{
        //    string receiptNo = acctReceiptService.GenerateReceiptNo();

        //    return Ok(new { receiptNo });
        //}

        [HttpPost("GenerateReceiptNo")]
        [Authorize]
        public IActionResult GenerateReceiptNo(AcctReceiptModel receipt)
        {
            string receiptNo = acctReceiptService.GenerateReceiptNoV2(receipt, currentUser.OfficeCode);

            return Ok(new { receiptNo });
        }

        [HttpPost("GetInvoiceForReceipt")]
        [Authorize]
        public IActionResult GetInvoiceForReceipt(ReceiptInvoiceCriteria criteria)
        {
            List<ReceiptInvoiceModel> results = acctReceiptService.GetInvoiceForReceipt(criteria);
            return Ok(new ResultHandle { Data = results, Status = results.Count > 0 ? true : false });
        }

        /// <summary>
        /// Get detail receipt
        /// </summary>
        /// <param name="id">id of receipt</param>
        /// <returns></returns>
        [HttpGet("GetById")]
        [Authorize]
        public IActionResult GetById(Guid id)
        {
            var detail = acctReceiptService.GetById(id);
            return Ok(detail);
        }

        /// <summary>
        /// Get detail receipt combine
        /// </summary>
        /// <param name="arcbNo"></param>
        /// <returns></returns>
        [HttpGet("GetByReceiptCombine")]
        [Authorize]
        public IActionResult GetByReceiptCombine(string arcbNo)
        {
            var detail = acctReceiptService.GetByReceiptCombine(arcbNo);
            return Ok(detail);
        }

        [HttpDelete]
        [Authorize]
        public IActionResult Delete(Guid id)
        {
            currentUser.Action = "DeleteReceipt";
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.acctARP);
            PermissionRange permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Delete);

            if (!acctReceiptService.CheckAllowPermissionAction(id, permissionRange))
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.DO_NOT_HAVE_PERMISSION].Value });
            }

            HandleState hs = acctReceiptService.Delete(id);
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Update Cancel Receipt
        /// </summary>
        /// <param name="id">id of receipt</param>
        /// <returns></returns>
        /// 
        [HttpPut("CancelReceipt/{id}")]
        public IActionResult CancelReceipt(Guid id)
        {
            var hs = acctReceiptService.SaveCancel(id);

            if (!hs.Success)
            {
                ResultHandle _result = new ResultHandle { Status = hs.Success, Message = hs.Message.ToString(), Data = id };
                return BadRequest(_result);
            }
          
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };

            if (hs.Success)
            {
                Response.OnCompleted(async () =>
                {
                    var modelReceivableList = acctReceiptService.GetListReceivableReceipt(id);
                    if(modelReceivableList.Count > 0)
                    {
                        await _busControl.SendAsync(RabbitExchange.EFMS_Accounting, RabbitConstants.CalculatingReceivableDataPartnerQueue, modelReceivableList);
                    }                    
                });
            }
            return Ok(result);

            
        }

        /// <summary>
        /// Save Receipt
        /// </summary>
        /// <param name="receiptModel"></param>
        /// <param name="saveAction">
        /// 0 - Save Draft - Add
        /// 1 - Save Draft - Update
        /// 2 - Save Done
        /// 3 - Save Cancel
        /// </param>
        /// <returns></returns>
        [HttpPost("SaveReceipt")]
        [Authorize]
        public IActionResult SaveReceipt(AcctReceiptModel receiptModel, SaveAction saveAction)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (!string.IsNullOrEmpty(receiptModel.PaymentRefNo))
            {
                if (!ValidateReceiptNo(receiptModel.Id, receiptModel.PaymentRefNo, receiptModel.PaymentDate))
                {
                    string mess = String.Format("Receipt {0} have existed", receiptModel.PaymentRefNo);
                    var _result = new { Status = false, Message = mess, Data = receiptModel, Code = 409 };
                    return BadRequest(_result);
                }
            }
            else
            {
                string receiptNo = acctReceiptService.GenerateReceiptNoV2(receiptModel, currentUser.OfficeCode);
                receiptModel.PaymentRefNo = receiptNo;
            }
            var hsDuplicate = acctReceiptService.CheckExitedCombineReceipt(new List<AcctReceiptModel>() { receiptModel });
            if (!hsDuplicate.Success)
            {
                return BadRequest(new ResultHandle { Status = false, Message = hsDuplicate.Message?.ToString() });
            }
            if (saveAction == SaveAction.SAVEDONE)
            {
                var hsCheckDone = CheckValidReciptSaveDone(new List<AcctReceiptModel>() { receiptModel });
                if (hsCheckDone != null)
                {
                    return BadRequest(hsCheckDone);
                }
                #region delete old
                //if (receiptModel.Id == Guid.Empty && receiptModel.ReferenceId != null)
                //{
                //    bool isExisted = acctReceiptService.Any(x => x.ReferenceId == receiptModel.ReferenceId
                //    && x.Status != AccountingConstants.RECEIPT_STATUS_CANCEL
                //    && x.PaymentMethod == AccountingConstants.PAYMENT_METHOD_MANAGEMENT_FEE);
                //    if (isExisted == true)
                //    {
                //        string receiptNo = acctReceiptService.First(x => x.ReferenceId == receiptModel.ReferenceId).ReferenceNo;
                //        string mess = String.Format("This Receipt already had Bank Fee/ Other fee Receipt {0}", receiptNo);
                //        var _result = new { Status = false, Message = mess, Data = receiptModel, Code = 409 };
                //        return BadRequest(_result);
                //    }
                //}

                //if (receiptModel.PaymentMethod == AccountingConstants.PAYMENT_METHOD_COLL_INTERNAL)
                //{
                //    bool isValidCusAgreement = acctReceiptService.ValidateCusAgreement(receiptModel.AgreementId ?? new Guid(), receiptModel.PaidAmountVnd ?? 0, receiptModel.PaidAmountUsd ?? 0);
                //    if (!isValidCusAgreement)
                //    {
                //        string mess = String.Format("Your Clear Amount > The Current advance of Partner, Pls check it again!");
                //        var _result = new { Status = false, Message = mess, Data = receiptModel, Code = 407 };
                //        return BadRequest(_result);
                //    }
                //}
                //if ((receiptModel.CusAdvanceAmountVnd ?? 0) > 0 || (receiptModel.CusAdvanceAmountUsd ?? 0) > 0)
                //{
                //    bool isValidCusAgreement = acctReceiptService.ValidateCusAgreement(receiptModel.AgreementId ?? new Guid(), receiptModel.CusAdvanceAmountVnd ?? 0, receiptModel.CusAdvanceAmountUsd ?? 0);
                //    if (!isValidCusAgreement)
                //    {
                //        string mess = String.Format("Cus Advance Amount in Receipt > The current Advance of Partner , Please check it again!");
                //        var _result = new { Status = false, Message = mess, Data = receiptModel, Code = 408 };
                //        return BadRequest(_result);
                //    }
                //}
                //string ListPaymentMessageInvalid = ValidatePaymentList(receiptModel, receiptModel.Payments);
                //if (!string.IsNullOrWhiteSpace(ListPaymentMessageInvalid))
                //{
                //    ResultHandle _result = new ResultHandle { Status = false, Message = ListPaymentMessageInvalid, Data = receiptModel };
                //    return BadRequest(_result);
                //}

                //string msgCheckPaidPayment = CheckInvoicePaid(receiptModel);
                //if (msgCheckPaidPayment.Length > 0)
                //{
                //    return BadRequest(new ResultHandle { Status = false, Message = msgCheckPaidPayment });
                //}
                #endregion
            }

            HandleState hs = acctReceiptService.SaveReceipt(receiptModel, saveAction);

            ResultHandle result = new ResultHandle();
            string message = string.Empty;
            switch (saveAction)
            {
                case SaveAction.SAVEDRAFT_ADD:
                case SaveAction.SAVEBANK_ADD:
                    message = HandleError.GetMessage(hs, Crud.Insert);
                    result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = receiptModel };
                    break;
                case SaveAction.SAVEDRAFT_UPDATE:
                    message = HandleError.GetMessage(hs, Crud.Update);
                    result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = receiptModel };
                    break;
                case SaveAction.SAVEDONE:
                    result = new ResultHandle { Status = hs.Success, Message = hs.Success ? "Save Done Receipt Successful" : hs.Message.ToString(), Data = receiptModel };
                    break;
                case SaveAction.SAVECANCEL:
                    result = new ResultHandle { Status = hs.Success, Message = hs.Success ? "Save Cancel Receipt Successful" : hs.Message.ToString(), Data = receiptModel };
                    break;
                default:
                    result = new ResultHandle { Status = false, Message = "Save Receipt fail" };
                    break;
            }

            if (!hs.Success)
            {
                return BadRequest(result);
            }
            else if (saveAction == SaveAction.SAVECANCEL || saveAction == SaveAction.SAVEDONE)
            {
                Response.OnCompleted(async () =>
                {
                    var modelReceivableList = acctReceiptService.GetListReceivableReceipt(receiptModel.Id);
                    if (modelReceivableList.Count > 0)
                    {
                        await _busControl.SendAsync(RabbitExchange.EFMS_Accounting, RabbitConstants.CalculatingReceivableDataPartnerQueue, modelReceivableList);
                    }
                    if (saveAction == SaveAction.SAVEDONE && !string.IsNullOrEmpty(receiptModel.NotifyDepartment))
                    {
                        List<int> deptIds = receiptModel.NotifyDepartment.Split(",").Select(x => Int32.Parse(x)).Distinct().ToList();
                        acctReceiptService.AlertReceiptToDeppartment(deptIds, receiptModel);
                    }
                    await CalculateOverDueAsync(new List<string>() { receiptModel.CustomerId });
                });

                if (receiptModel.PaymentMethod.ToLower().Contains("credit"))
                {
                    var hsCredit = acctReceiptService.UpdateCreditARCombine(new List<AcctReceiptModel>() { receiptModel }, saveAction);
                    if (!hsCredit.Success)
                    {
                        new LogHelper("eFMS_SaveReceipt_UpdateCreditARCombine_LOG", hsCredit.Message?.ToString() + " - Data:" + JsonConvert.SerializeObject(receiptModel));
                    }
                    hsCredit = acctReceiptService.AddPaymentsCreditCombine(new List<AcctReceiptModel>() { receiptModel }, saveAction);
                    if (!hsCredit.Success)
                    {
                        new LogHelper("eFMS_SaveReceipt_AddPaymentsCreditCombine_LOG", hsCredit.Message?.ToString() + " - Data:" + JsonConvert.SerializeObject(receiptModel));
                    }
                }
            }
            if (saveAction == SaveAction.SAVEDRAFT_ADD || saveAction == SaveAction.SAVEDRAFT_UPDATE || saveAction == SaveAction.SAVEDONE)
            {
                // cập nhật bảng debit ar
                bool isCalDebit = false;
                if (!string.IsNullOrEmpty(receiptModel.Arcbno))
                {
                    isCalDebit = receiptModel.PaymentMethod.ToLower().Contains("debit");
                }
                else
                {
                    isCalDebit = true;
                }

                // Cập nhật cấn trừ debit
                if (receiptModel.Type == "Agent" && isCalDebit)
                {
                    var hsDebit = acctReceiptService.UpdateAccountingDebitAR(receiptModel.Payments, receiptModel.Id, saveAction);
                    if (!hsDebit.Success)
                    {
                        new LogHelper("eFMS_SaveReceipt_UpdateDebitAR_LOG", hsDebit.Message?.ToString() + " - Data:" + JsonConvert.SerializeObject(receiptModel));
                    }
                }
            }
            return Ok(result);
        }

        /// <summary>
        /// check receipt is valid when save done receipt
        /// </summary>
        /// <param name="receiptModels"></param>
        /// <returns></returns>
        private object CheckValidReciptSaveDone(List<AcctReceiptModel> receiptModels)
        {
            foreach (var receiptModel in receiptModels)
            {
                if (receiptModel.Id == Guid.Empty && receiptModel.ReferenceId != null)
                {
                    bool isExisted = acctReceiptService.Any(x => x.ReferenceId == receiptModel.ReferenceId
                    && x.Status != AccountingConstants.RECEIPT_STATUS_CANCEL
                    && x.PaymentMethod == AccountingConstants.PAYMENT_METHOD_MANAGEMENT_FEE);
                    if (isExisted == true)
                    {
                        string receiptNo = acctReceiptService.First(x => x.ReferenceId == receiptModel.ReferenceId).ReferenceNo;
                        string mess = String.Format("This Receipt already had Bank Fee/ Other fee Receipt {0}", receiptNo);
                        var _result = new { Status = false, Message = mess, Data = receiptModel, Code = 409 };
                        return (_result);
                    }
                }

                if (receiptModel.PaymentMethod == AccountingConstants.PAYMENT_METHOD_COLL_INTERNAL)
                {
                    bool isValidCusAgreement = acctReceiptService.ValidateCusAgreement(receiptModel.AgreementId ?? new Guid(), receiptModel.PaidAmountVnd ?? 0, receiptModel.PaidAmountUsd ?? 0);
                    if (!isValidCusAgreement)
                    {
                        string mess = String.Format("Your Clear Amount > The Current advance of Partner, Pls check it again!");
                        var _result = new { Status = false, Message = mess, Data = receiptModel, Code = 407 };
                        return (_result);
                    }
                }
                if ((receiptModel.CusAdvanceAmountVnd ?? 0) > 0 || (receiptModel.CusAdvanceAmountUsd ?? 0) > 0)
                {
                    bool isValidCusAgreement = acctReceiptService.ValidateCusAgreement(receiptModel.AgreementId ?? new Guid(), receiptModel.CusAdvanceAmountVnd ?? 0, receiptModel.CusAdvanceAmountUsd ?? 0);
                    if (!isValidCusAgreement)
                    {
                        string mess = String.Format("Cus Advance Amount in Receipt > The current Advance of Partner , Please check it again!");
                        var _result = new { Status = false, Message = mess, Data = receiptModel, Code = 408 };
                        return (_result);
                    }
                }
                string ListPaymentMessageInvalid = ValidatePaymentList(receiptModel, receiptModel.Payments);
                if (!string.IsNullOrWhiteSpace(ListPaymentMessageInvalid))
                {
                    ResultHandle _result = new ResultHandle { Status = false, Message = ListPaymentMessageInvalid, Data = receiptModel };
                    return (_result);
                }

                string msgCheckPaidPayment = CheckInvoicePaid(receiptModel);
                if (msgCheckPaidPayment.Length > 0)
                {
                    return (new ResultHandle { Status = false, Message = msgCheckPaidPayment });
                }
            }
            return null;
        }

        /// <summary>
        /// Save Combine Receipt
        /// </summary>
        /// <param name="receiptModels"></param>
        /// <param name="saveAction">
        /// 0 - Save Draft - Add
        /// 1 - Save Draft - Update
        /// 2 - Save Done
        /// 3 - Save Cancel
        /// </param>
        /// <returns></returns>
        [HttpPost("SaveCombineReceipt")]
        [Authorize]
        public IActionResult SaveCombineReceipt(List<AcctReceiptModel> receiptModels, SaveAction saveAction)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            receiptModels = receiptModels.Where(x => string.IsNullOrEmpty(x.Status) || x.Status == "Draft").ToList();
            if (receiptModels.Count == 0)
                return BadRequest(ModelState);

            var _validReceiptNos = CheckValidReceiptNo(receiptModels.Where(x => !string.IsNullOrEmpty(x.PaymentRefNo)).ToList());
            if (_validReceiptNos != null)
            {
                return Ok(_validReceiptNos);
            }
            ResultHandle result = new ResultHandle();
            var hsDuplicate = acctReceiptService.CheckExitedCombineReceipt(receiptModels);
            if (!hsDuplicate.Success)
            {
                return Ok(new ResultHandle { Status = false, Message = hsDuplicate.Message?.ToString() });
            }
            if (saveAction == SaveAction.SAVEDONE)
            {
                var hsCheckDone = CheckValidReciptSaveDone(receiptModels);
                if (hsCheckDone != null)
                {
                    return Ok(hsCheckDone);
                }
            }
            var debitReceipts = receiptModels.Where(x => (string.IsNullOrEmpty(x.Status) || x.Status == "Draft") && x.PaymentMethod.ToLower().Contains("debit")).ToList();
            var creditReceipts = receiptModels.Where(x => (string.IsNullOrEmpty(x.Status) || x.Status == "Draft") && x.PaymentMethod.ToLower().Contains("credit")).ToList();

            HandleState hs = acctReceiptService.SaveCombineReceipt(receiptModels, saveAction);
            
            string message = string.Empty;
            switch (saveAction)
            {
                case SaveAction.SAVEDRAFT_ADD:
                case SaveAction.SAVEBANK_ADD:
                    message = HandleError.GetMessage(hs, Crud.Insert);
                    result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = receiptModels };
                    break;
                case SaveAction.SAVEDRAFT_UPDATE:
                    message = HandleError.GetMessage(hs, Crud.Update);
                    result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = receiptModels };
                    break;
                case SaveAction.SAVEDONE:
                    result = new ResultHandle { Status = hs.Success, Message = hs.Success ? "Save Done Receipt Successful" : hs.Message.ToString(), Data = receiptModels };
                    break;
                case SaveAction.SAVECANCEL:
                    result = new ResultHandle { Status = hs.Success, Message = hs.Success ? "Save Cancel Receipt Successful" : hs.Message.ToString(), Data = receiptModels };
                    break;
                default:
                    result = new ResultHandle { Status = false, Message = "Save Receipt fail" };
                    break;
            }
            
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            else if (saveAction == SaveAction.SAVECANCEL || saveAction == SaveAction.SAVEDONE)
            {
                Response.OnCompleted(async () =>
                {
                    foreach (var receiptModel in debitReceipts)
                    {
                        var modelReceivableList = acctReceiptService.GetListReceivableReceipt(receiptModel.Id);
                        if (modelReceivableList.Count > 0)
                        {
                            await _busControl.SendAsync(RabbitExchange.EFMS_Accounting, RabbitConstants.CalculatingReceivableDataPartnerQueue, modelReceivableList);
                        }
                        await CalculateOverDueAsync(new List<string>() { receiptModel.CustomerId });
                    }
                });
                if (receiptModels.Any(x => x.PaymentMethod.ToLower().Contains("credit")))
                {
                    var hsCredit = acctReceiptService.UpdateCreditARCombine(creditReceipts, saveAction);
                    if (!hsCredit.Success)
                    {
                        new LogHelper("eFMS_SaveReceipt_UpdateCreditARCombine_LOG", hsCredit.Message?.ToString() + " - Data:" + JsonConvert.SerializeObject(creditReceipts));
                    }
                    hsCredit = acctReceiptService.AddPaymentsCreditCombine(creditReceipts, saveAction);
                    if (!hsCredit.Success)
                    {
                        new LogHelper("eFMS_SaveReceipt_AddPaymentsCreditCombine_LOG", hsCredit.Message?.ToString() + " - Data:" + JsonConvert.SerializeObject(creditReceipts));
                    }
                }
            }
            if (saveAction == SaveAction.SAVEDRAFT_ADD || saveAction == SaveAction.SAVEDRAFT_UPDATE || saveAction == SaveAction.SAVEDONE)
            {
                // Cập nhật cấn trừ debit
                foreach (var receiptModel in debitReceipts)
                {
                    var hsDebit = acctReceiptService.UpdateAccountingDebitAR(receiptModel.Payments, receiptModel.Id, saveAction);
                    if (!hsDebit.Success)
                    {
                        new LogHelper("eFMS_SaveReceipt_UpdateDebitAR_LOG", hsDebit.Message?.ToString() + " - Data:" + JsonConvert.SerializeObject(receiptModel));
                    }
                }
            }
            return Ok(result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="receiptNos"></param>
        /// <returns></returns>
        private object CheckValidReceiptNo(List<AcctReceiptModel> receiptNos)
        {
            foreach(var rcpNo in receiptNos)
            {
                if (!ValidateReceiptNo(rcpNo.Id, rcpNo.PaymentRefNo, rcpNo.PaymentDate))
                {
                    string mess = String.Format("Receipt {0} have existed", rcpNo.PaymentRefNo);
                    var _result = new { Status = false, Message = mess, Data = rcpNo };
                    return _result;
                }
            }
            return null;
        }

        [HttpPut("SaveDoneReceipt")]
        public IActionResult SaveDoneReceipt(Guid receiptId)
        {
            var hs = acctReceiptService.SaveDoneReceipt(receiptId);
            //if (hs.Success)
            //{
            //    //Tính công nợ sau khi Save Done thành công
            //    acctReceiptService.CalculatorReceivableForReceipt(receiptId);
            //}
            ResultHandle result = new ResultHandle();
            if (!hs.Success)
            {
                ResultHandle _result = new ResultHandle { Status = hs.Success, Message = hs.Message.ToString(), Data = receiptId };
                return BadRequest(_result);
            }
            else
            {
                var message = HandleError.GetMessage(hs, Crud.Update);
                result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };

                Response.OnCompleted(async () =>
                {
                    var modelReceivableList = acctReceiptService.GetListReceivableReceipt(receiptId);
                    if (modelReceivableList.Count > 0)
                    {
                        await _busControl.SendAsync(RabbitExchange.EFMS_Accounting, RabbitConstants.CalculatingReceivableDataPartnerQueue, modelReceivableList);
                    }
                    var receipt = acctReceiptService.First(x => x.Id == receiptId);
                    if(receipt != null)
                    {
                        await CalculateOverDueAsync(new List<string>() { receipt.CustomerId });
                    }
                });
            }

            return Ok(result);
        }

        private async Task<IActionResult> CalculateOverDueAsync(List<string> partnerIds)
        {
            Uri urlAccounting = new Uri(apiServiceUrl.Value.Url);
            string accessToken = Request.Headers["Authorization"].ToString();

            HttpResponseMessage resquestOverDue1To15 = await HttpClientService.PutAPI(urlAccounting + "Accounting/api/v1/e/AccountReceivable/CalculateOverDue1To15", partnerIds, accessToken);
            HttpResponseMessage resquestOverDue15To30 = await HttpClientService.PutAPI(urlAccounting + "Accounting/api/v1/e/AccountReceivable/CalculateOverDue15To30", partnerIds, accessToken);
            HttpResponseMessage resquestOverDueover30 = await HttpClientService.PutAPI(urlAccounting + "Accounting/api/v1/e/AccountReceivable/CalculateOverDue30", partnerIds, accessToken);
            var responseOverDue1To15 = await resquestOverDue1To15.Content.ReadAsAsync<ResultHandle>();
            var responseOverDue15To30 = await resquestOverDue15To30.Content.ReadAsAsync<ResultHandle>();
            var responseOverDueOver30 = await resquestOverDueover30.Content.ReadAsAsync<ResultHandle>();

            return Ok();
        }

        [HttpPost("ProcessInvoice")]
        public IActionResult ProcessInvoice(ProcessReceiptInvoice criteria)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("invalid data");
            }
            ProcessClearInvoiceModel data = acctReceiptService.ProcessReceiptInvoice(criteria);
            return Ok(data);
        }

        [HttpGet("CheckAllowDetail/{id}")]
        [Authorize]
        public IActionResult CheckAllowDetail(Guid id)
        {
            var charge = acctReceiptService.First(x => x.Id == id);
            if (charge == null)
            {
                return Ok(false);
            }

            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.acctARP);
            PermissionRange permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Detail);

            return Ok(acctReceiptService.CheckAllowPermissionAction(id, permissionRange));
        }

        [HttpGet("CheckAllowDelete/{id}")]
        [Authorize]
        public IActionResult CheckAllowDelete(Guid id)
        {
            var charge = acctReceiptService.First(x => x.Id == id);
            if (charge == null)
            {
                return Ok(false);
            }
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.acctARP);
            PermissionRange permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Delete);

            return Ok(acctReceiptService.CheckAllowPermissionAction(id, permissionRange));
        }

        [HttpGet("CheckExistedReceiptNo")]
        public IActionResult CheckExistedReceiptNo(Guid Id, string receiptNo, DateTime paymentDate)
        {
            var isExisted = ValidateReceiptNo(Id, receiptNo, paymentDate);
            return Ok(!isExisted ? "Duplicated" : "not Duplicated");
        }

        private bool ValidateReceiptNo(Guid Id, string receiptNo, DateTime? paymentDate)
        {
            bool valid = true;
            if (Id == Guid.Empty)
            {
                valid = !acctReceiptService.Any(x => x.PaymentRefNo == receiptNo 
                && x.Status != AccountingConstants.RECEIPT_STATUS_CANCEL
                && x.PaymentDate.Value.Year == (paymentDate ?? DateTime.Now).Year
                );
            }
            else
            {
                valid = !acctReceiptService.Any(x => x.PaymentRefNo == receiptNo 
                && x.Id != Id 
                && x.Status != AccountingConstants.RECEIPT_STATUS_CANCEL
                && x.PaymentDate.Value.Year == (paymentDate ?? DateTime.Now).Year);
            }

            return valid;
        }

        private bool ValidateReceiptExistedReference(Guid Id, string reference)
        {
            bool valid = true;
            if(Id == Guid.Empty)
            {
                valid = !acctReceiptService.Any(x => x.ReferenceId.ToString() == reference);
            }
            return valid;
        }

        private string CheckInvoicePaid(AcctReceiptModel receiptModel)
        {
            string result = string.Empty;
            List<ReceiptInvoiceModel> payments = receiptModel.Payments.Where(x => x.PaymentType != "CREDIT" && x.PaymentType != "OTHER").ToList();
            bool isValidPayment = acctReceiptService.CheckPaymentPaid(payments);

            if (isValidPayment == true)
            {
                result = stringLocalizer[AccountingLanguageSub.MSG_RECEIPT_HAVE_PAYMENT_PAID].Value;
            }
            return result;
        }

        private string ValidatePaymentList(AcctReceiptModel model, List<ReceiptInvoiceModel> payments)
        {
            string messageInValid = string.Empty;
            if (payments.Count == 0)
            {
                messageInValid = "Receipt don't have any payment in this period, Please check it again!";
            }
            else
            {
                if (model.Class == AccountingConstants.RECEIPT_CLASS_CLEAR_DEBIT && !payments.Any(x => (x.Type == "DEBIT" || x.Type == "OBH")))
                {
                    messageInValid = "You can't save without debit in this period, Please check it again!";
                }

                if (payments.Any(x => (x.PaymentType == "CREDIT")))
                {
                    bool isHaveInvoice = payments.Any(x => x.Type == "DEBIT" && string.IsNullOrEmpty(x.InvoiceNo));
                    if (isHaveInvoice == true)
                    {
                        messageInValid = "Some credit do not have net off invoice";
                    }
                }

                if (payments.Any(x => x.Type == "DEBIT"
                                && x.TotalPaidVnd > 0
                                && string.IsNullOrEmpty(x.CreditNo)
                                && (x.TotalPaidVnd > x.UnpaidAmountVnd || x.TotalPaidUsd > x.UnpaidAmountUsd))
                                )
                {
                    List<ReceiptInvoiceModel> invalidPayments = model.Type.ToLower() == "customer" ?
                        payments.Where(x => x.Type == "DEBIT" && x.TotalPaidVnd > 0 && (x.TotalPaidVnd > x.UnpaidAmountVnd || x.TotalPaidUsd > x.UnpaidAmountUsd)).ToList() :
                        payments.Where(x => x.Type == "DEBIT" && x.TotalPaidUsd > 0 && x.TotalPaidUsd > x.UnpaidAmountUsd).ToList();
                    List<string> messages = new List<string>();
                    if (invalidPayments.Count > 0)
                    {
                        foreach (var item in invalidPayments)
                        {
                            messageInValid += string.Format(@"Invoice {0} Total Paid must <= Unpaid", item.InvoiceNo) + "\n";
                        }
                    }
                }
            }

            return messageInValid;
        }

        /// <summary>
        /// Get Data Issue Customer Payment
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPost("GetDataIssueCustomerPayment")]
        public IActionResult GetDataIssueCustomerPayment(CustomerDebitCreditCriteria criteria)
        {
            var result = acctReceiptService.GetDataIssueCustomerPayment(criteria);
            return Ok(result);
        }
        /// <summary>
        /// Get Data Issue Customer Payment
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPost("GetDataIssueAgencyPayment")]
        public IActionResult GetDataIssueAgencyPayment(CustomerDebitCreditCriteria criteria)
        {
            var result = acctReceiptService.GetDataIssueAgencyPayment(criteria);
            return Ok(result);
        }

        /// <summary>
        /// Get Data Issued Agency Payment
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPost("GetDataIssueCreditAgency")]
        public IActionResult GetDataIssueCreditAgency(CustomerDebitCreditCriteria criteria)
        {
            var result = acctReceiptService.GetDataIssueCreditAgency(criteria);
            return Ok(result);
        }

        [HttpPost("GetDataExportReceiptAdvance")]
        [Authorize]
        public async Task<IActionResult> GetDataExportReceiptAdvance(AcctReceiptCriteria criteria)
        {
            var result = await acctReceiptService.GetDataExportReceiptAdvance(criteria);

            return Ok(result);
        }

        [HttpPut("{Id}/QuickUpdate")]
        [Authorize]
        public async Task<IActionResult> QuickUpdate(Guid Id, ReceiptQuickUpdateModel model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (!ValidateReceiptNo(Id, model.PaymentRefNo, model.PaymentDate))
            {
                string mess = String.Format("Receipt {0} have existed", model.PaymentRefNo);
                var _result = new { Status = false, Message = mess, Data = model, Code = 409 };
                return BadRequest(_result);
            }
            HandleState hs = await acctReceiptService.QuickUpdate(Id, model);
            string message = HandleError.GetMessage(hs, Crud.Update);
            if(!hs.Success)
            {
                return BadRequest(new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = model });
            }

            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = model };
            return Ok(result);
        }
    }
}