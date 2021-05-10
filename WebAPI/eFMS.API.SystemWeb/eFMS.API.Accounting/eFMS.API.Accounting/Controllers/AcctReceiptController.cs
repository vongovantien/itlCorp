﻿using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.Criteria;
﻿using eFMS.API.Accounting.DL.Common;
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
            var result = new ResponsePagingModel<AcctReceiptModel> { Data = data, Page = page, Size = size };
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
            return Ok(new ResultHandle { Data = results , Status = results.Count > 0 ? true : false });
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

            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value};
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

            //Check exists list invoice/adv
            if (receiptModel.Payments.Count == 0)
            {
                ResultHandle _result = new ResultHandle { Status = false, Message = "Receipt don't have any payment in this period, Please check it again!", Data = receiptModel };
                return BadRequest(_result);
            }

            //Check exists invoice payment PAID
            if (receiptModel.Payments.Any(x => x.PaymentStatus == AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID))
            {
                ResultHandle _result = new ResultHandle { Status = false, Message = "Receipt existed Paid Invoice(s), Please you check and remove them!", Data = receiptModel };
                return BadRequest(_result);
            }

            var hs = acctReceiptService.SaveReceipt(receiptModel, saveAction);

            ResultHandle result = new ResultHandle();
            if (saveAction == SaveAction.SAVEDRAFT_ADD)
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
                if (hs.Success)
                {
                    //Tính công nợ sau khi Save Done thành công
                    acctReceiptService.CalculatorReceivableForReceipt(receiptModel.Id);
                }
                result = new ResultHandle { Status = hs.Success, Message = "Save Done Receipt Successful", Data = receiptModel };
            } 
            else if (saveAction == SaveAction.SAVECANCEL)
            {
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
            return Ok(result);
        }

        [HttpPut("SaveDoneReceipt")]
        public IActionResult SaveDoneReceipt(Guid receiptId)
        {
            var hs = acctReceiptService.SaveDoneReceipt(receiptId);
            if (hs.Success)
            {
                //Tính công nợ sau khi Save Done thành công
                acctReceiptService.CalculatorReceivableForReceipt(receiptId);
            }
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
            if(Id == Guid.Empty)
            {
                valid = !acctReceiptService.Any(x => x.PaymentRefNo == receiptNo);
            }
            else
            {
                valid = !acctReceiptService.Any(x => x.PaymentRefNo == receiptNo && x.Id != Id);
            }

            return valid;
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
    }
}