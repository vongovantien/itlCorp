using eFMS.API.Accounting.DL.Common;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.Criteria;
using eFMS.API.Accounting.Infrastructure.Middlewares;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Infrastructure.Common;
using ITL.NetCore.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System;
using System.Linq;

namespace eFMS.API.Accounting.Controllers
{
    /// <summary>
    /// A base class for an MVC controller without view support.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class AccountingManagementController: ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly IAccountingManagementService accountingService;
        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="accService"></param>
        public AccountingManagementController(IStringLocalizer<LanguageSub> localizer,
            IAccountingManagementService accService)
        {
            stringLocalizer = localizer;
            accountingService = accService;
        }

        [Authorize]
        [HttpGet("CheckAllowDelete")]
        public IActionResult CheckAllowDelete(Guid id)
        {
            var result = accountingService.CheckDeletePermission(id);
            if (result == 403)
            {
                return Ok(false);
            }
            return Ok(true);
        }

        [HttpDelete]
        [Authorize]
        public IActionResult Delete(Guid id)
        {
            HandleState hs = accountingService.Delete(id);
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
        /// Get charges sell (Debit) to issue Invoice by criteria
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPost("GetChargeSellForInvoiceByCriteria")]
        public IActionResult GetChargeSellForInvoiceByCriteria(PartnerOfAcctManagementCriteria criteria)
        {
            var result = accountingService.GetChargeSellForInvoiceByCriteria(criteria);
            return Ok(result);
        }

        /// <summary>
        /// Get charges to issue Voucher by criteria
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPost("GetChargeForVoucherByCriteria")]
        public IActionResult GetChargeForVoucherByCriteria(PartnerOfAcctManagementCriteria criteria)
        {
            var result = accountingService.GetChargeForVoucherByCriteria(criteria);
            return Ok(result);
        }

        /// <summary>
        /// get and paging the list of Accounting Management by conditions
        /// </summary>
        /// <param name="criteria">search conditions</param>
        /// <param name="pageNumber">page to retrieve data</param>
        /// <param name="pageSize">number items per page</param>
        /// <returns></returns>
        [HttpPost]
        [Route("Paging")]
        [Authorize]
        public IActionResult Paging(AccAccountingManagementCriteria criteria, int pageNumber, int pageSize)
        {
            var data = accountingService.Paging(criteria, pageNumber, pageSize, out int totalItems);
            var result = new { data, totalItems, pageNumber, pageSize };
            return Ok(result);
        }

        /// <summary>
        /// Add new Accounting Management (Invoice or Voucher)
        /// </summary>
        /// <param name="model">object to add</param>
        /// <returns></returns>
        [HttpPost]
        [Route("Add")]
        [Authorize]
        public IActionResult Add(AccAccountingManagementModel model)
        {
            if (!ModelState.IsValid) return BadRequest();

            var isExisedVoucherId = CheckExistedVoucherId(model.VoucherId, model.Id);
            if (isExisedVoucherId)
            {
                return BadRequest(new ResultHandle { Status = false, Message = "Voucher ID has been existed" });
            }

            var isExistedInvoiceNoTempSerie = CheckExistedInvoiceNoTempSerie(model.InvoiceNoTempt, model.Serie, model.Id);
            if (isExistedInvoiceNoTempSerie)
            {
                return BadRequest(new ResultHandle { Status = false, Message = "Invoice No (Tempt) - Seri has been existed" });
            }

            if (model.Charges.Count == 0)
            {
                string accountType = model.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE ? "VAT Invoice" : "Voucher";
                ResultHandle _result = new ResultHandle { Status = false, Message = accountType + " don't have any charge in this period, Please check it again!" };
                return BadRequest(_result);
            }

            var hs = accountingService.AddAcctMgnt(model);
            
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = model };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Update Accounting Management (Invoice or Voucher)
        /// </summary>
        /// <param name="model">object to update</param>
        /// <returns></returns>
        [HttpPut]
        [Route("Update")]
        [Authorize]
        public IActionResult Update(AccAccountingManagementModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var isExisedVoucherId = CheckExistedVoucherId(model.VoucherId, model.Id);
            if (isExisedVoucherId)
            {
                return BadRequest(new ResultHandle { Status = false, Message = "Voucher ID has been existed" });
            }

            var isExistedInvoiceNoTempSerie = CheckExistedInvoiceNoTempSerie(model.InvoiceNoTempt, model.Serie, model.Id);
            if (isExistedInvoiceNoTempSerie)
            {
                return BadRequest(new ResultHandle { Status = false, Message = "Invoice No (Tempt) - Seri has been existed" });
            }

            if (model.Charges.Count == 0)
            {
                string accountType = model.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE ? "VAT Invoice" : "Voucher";
                ResultHandle _result = new ResultHandle { Status = false, Message = accountType + " don't have any charge in this period, Please check it again!" };
                return BadRequest(_result);
            }

            var hs = accountingService.UpdateAcctMngt(model);
            
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = model };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpGet("CheckVoucherIdExist")]
        public IActionResult CheckVoucherIdExist(string voucherId, Guid? acctId)
        {
            var isExited = CheckExistedVoucherId(voucherId, acctId);
            return Ok(isExited);
        }

        private bool CheckExistedVoucherId(string voucherId, Guid? acctId)
        {
            var isExited = false;
            if (acctId == Guid.Empty || acctId == null)
            {
                isExited = accountingService.Get(x => x.VoucherId == voucherId).Any();
            }
            else
            {
                isExited = accountingService.Get(x => x.VoucherId == voucherId && x.Id != acctId).Any();
            }
            return isExited;
        }

        [HttpGet("GenerateVoucherId")]
        public IActionResult GenerateVoucherId()
        {
            var voucherId = accountingService.GenerateVoucherId();
            return Ok(new { VoucherId = voucherId });
        }

        [HttpGet("GetById")]
        public IActionResult GetDetailById(Guid id)
        {
            var detail = accountingService.GetById(id);
            return Ok(detail);
        }

        [HttpGet("GenerateInvoiceNoTemp")]
        public IActionResult GenerateInvoiceNoTemp()
        {
            var invoiceNoTemp = accountingService.GenerateInvoiceNoTemp();
            return Ok(new { InvoiceNoTemp = invoiceNoTemp });
        }

        [HttpGet("CheckInvoiceNoTempSerieExist")]
        public IActionResult CheckInvoiceNoTempSerieExist(string invoiceNoTemp, string serie, Guid? acctId)
        {
            var isExited = CheckExistedInvoiceNoTempSerie(invoiceNoTemp, serie, acctId);
            return Ok(isExited);
        }

        private bool CheckExistedInvoiceNoTempSerie(string invoiceNoTemp, string serie, Guid? acctId)
        {
            var isExited = false;
            if (acctId == Guid.Empty || acctId == null)
            {
                isExited = accountingService.Get(x => x.InvoiceNoTempt == invoiceNoTemp && x.Serie == serie).Any();
            }
            else
            {
                isExited = accountingService.Get(x => x.InvoiceNoTempt == invoiceNoTemp && x.Serie == serie && x.Id != acctId).Any();
            }
            return isExited;
        }
    }
}
