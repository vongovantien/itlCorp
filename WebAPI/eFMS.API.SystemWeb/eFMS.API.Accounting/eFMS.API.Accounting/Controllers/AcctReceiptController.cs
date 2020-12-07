using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.Criteria;
﻿using eFMS.API.Accounting.DL.Common;
using eFMS.API.Accounting.DL.Models.Receipt;
using eFMS.API.Accounting.Infrastructure.Middlewares;
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
        public AcctReceiptController(IStringLocalizer<LanguageSub> localizer,
           IAcctReceiptService acctReceipt)
        {
            stringLocalizer = localizer;
            acctReceiptService = acctReceipt;
        }

        [HttpPost]
        [Route("Query")]
        public IActionResult Query(AcctReceiptCriteria criteria)
        {
            IQueryable<AcctReceiptModel> result = acctReceiptService.Query(criteria);

            return Ok(result);
        }

        [HttpPost]
        [Route("Paging")]
        // [Authorize]
        public IActionResult Paging(AcctReceiptCriteria criteria, int page, int size)
        {
            IQueryable<AcctReceiptModel> data = acctReceiptService.Paging(criteria, page, size, out int rowsCount);
            var result = new ResponsePagingModel<AcctReceiptModel> { Data = null, Page = page, Size = size };
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

        [HttpGet("GetById")]
        [Authorize]
        public IActionResult GetById(Guid id)
        {
            var detail = acctReceiptService.GetById(id);
            return Ok(detail);
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

            //Check valid data amount: nếu Tổng Paid Amount (không bao gồm ADV) + Balance trên Payment List # Final Paid Amount => Thông báo lỗi
            var paidAmount = receiptModel.Payments.Select(s => s.PaidAmount + s.InvoiceBalance).Sum();
            if (paidAmount != receiptModel.FinalPaidAmount)
            {
                ResultHandle _result = new ResultHandle { Status = false, Message = "Total Paid Amount is not matched with Final Paid Amount, Please check it and Click Process Clear to update new value!", Data = receiptModel };
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
                result = new ResultHandle { Status = hs.Success, Message = "Save Done Successful", Data = receiptModel };
            } 
            else if (saveAction == SaveAction.SAVECANCEL)
            {
                result = new ResultHandle { Status = hs.Success, Message = "Save Cancel Successful", Data = receiptModel };
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
    }
}