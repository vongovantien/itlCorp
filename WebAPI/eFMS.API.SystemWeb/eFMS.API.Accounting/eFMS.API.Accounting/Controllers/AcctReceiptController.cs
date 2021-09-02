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


        public AcctReceiptController(IStringLocalizer<LanguageSub> localizer,
            ICurrentUser curUser,
           IAcctReceiptService acctReceipt)
        {
            stringLocalizer = localizer;
            acctReceiptService = acctReceipt;
            currentUser = curUser;
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


        [HttpGet("GenerateReceiptNo")]
        [Authorize]
        public IActionResult GenerateReceiptNo()
        {
            string receiptNo = acctReceiptService.GenerateReceiptNo();

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
            if (hs.Success)
            {
                //Tính công nợ sau khi Save Cancel thành công
                // acctReceiptService.CalculatorReceivableForReceipt(id);

                Response.OnCompleted(async () =>
                {
                    await acctReceiptService.CalculatorReceivableForReceipt(id);

                });
            }
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
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
            if (!ModelState.IsValid) return BadRequest();

            if (!ValidateReceiptNo(receiptModel.Id, receiptModel.PaymentRefNo))
            {
                string mess = String.Format("Receipt {0} have existed", receiptModel.PaymentRefNo);
                var _result = new { Status = false, Message = mess, Data = receiptModel, Code = 409 };
                return BadRequest(_result);
            }

            string ListPaymentMessageInvalid = ValidatePaymentList(receiptModel, receiptModel.Payments);
            if (!string.IsNullOrWhiteSpace(ListPaymentMessageInvalid))
            {
                ResultHandle _result = new ResultHandle { Status = false, Message = ListPaymentMessageInvalid, Data = receiptModel };
                return BadRequest(_result);
            }

            //Check exists invoice payment PAID
            if (saveAction != SaveAction.SAVECANCEL)
            {
                string msgCheckPaidPayment = CheckInvoicePaid(receiptModel);
                if (msgCheckPaidPayment.Length > 0)
                {
                    return BadRequest(new ResultHandle { Status = false, Message = msgCheckPaidPayment });
                }
            }

            var hs = acctReceiptService.SaveReceipt(receiptModel, saveAction);

            ResultHandle result = new ResultHandle();
            if (saveAction == SaveAction.SAVEDRAFT_ADD || saveAction == SaveAction.SAVEBANK_ADD)
            {
                var message = HandleError.GetMessage(hs, Crud.Insert);
                result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = receiptModel };
            }
            else if (saveAction == SaveAction.SAVEDRAFT_UPDATE)
            {
                var message = HandleError.GetMessage(hs, Crud.Update);
                result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = receiptModel };
            }
            else if (saveAction == SaveAction.SAVEDONE)
            {
                //if (hs.Success)
                //{
                //    //Tính công nợ sau khi Save Done thành công
                //    acctReceiptService.CalculatorReceivableForReceipt(receiptModel.Id);
                //}
                result = new ResultHandle { Status = hs.Success, Message = "Save Done Receipt Successful", Data = receiptModel };
            }
            else if (saveAction == SaveAction.SAVECANCEL)
            {
                //if (hs.Success)
                //{
                //    //Tính công nợ sau khi Save Cancel thành công
                //    acctReceiptService.CalculatorReceivableForReceipt(receiptModel.Id);
                //}
                result = new ResultHandle { Status = hs.Success, Message = "Save Cancel Receipt Successful", Data = receiptModel };
            }
            else
            {
                return BadRequest(new ResultHandle { Status = false, Message = "Save Receipt fail" });
            }

            if (!hs.Success)
            {
                ResultHandle _result = new ResultHandle { Status = hs.Success, Message = hs.Message.ToString(), Data = receiptModel };
                return BadRequest(_result);
            }
            else
            {
                Response.OnCompleted(async () =>
                {
                    await acctReceiptService.CalculatorReceivableForReceipt(receiptModel.Id);

                });
            }
            return Ok(result);
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
                    await acctReceiptService.CalculatorReceivableForReceipt(receiptId);

                });
            }


            return Ok(result);
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

        private bool ValidateReceiptNo(Guid Id, string receiptNo)
        {
            bool valid = true;
            if (Id == Guid.Empty)
            {
                valid = !acctReceiptService.Any(x => x.PaymentRefNo == receiptNo);
            }
            else
            {
                valid = !acctReceiptService.Any(x => x.PaymentRefNo == receiptNo && x.Id != Id);
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
                    List<ReceiptInvoiceModel> invalidPayments = payments.Where(x => x.Type == "DEBIT" && x.TotalPaidVnd > 0
                    && (x.TotalPaidVnd > x.UnpaidAmountVnd || x.TotalPaidUsd > x.UnpaidAmountUsd)).ToList();
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
    }
}