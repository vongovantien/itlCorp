using eFMS.API.Accounting.DL.Common;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.Criteria;
using eFMS.API.Accounting.Infrastructure.Middlewares;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Helpers;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.Accounting.Controllers
{
    /// <summary>
    /// A base class for an MVC controller without view support.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class AccountingManagementController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly IAccountingManagementService accountingService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ICurrentUser currentUser;
        private readonly IAccAccountReceivableService accountReceivableService;
        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="hostingEnvironment"></param>
        /// <param name="accService"></param>
        /// <param name="currUser"></param>
        public AccountingManagementController(
            IStringLocalizer<LanguageSub> localizer,
            IHostingEnvironment hostingEnvironment,
            IAccountingManagementService accService,
            IAccAccountReceivableService accountReceivable,
            ICurrentUser currUser)
        {
            stringLocalizer = localizer;
            accountingService = accService;
            _hostingEnvironment = hostingEnvironment;
            currentUser = currUser;
            accountReceivableService = accountReceivable;
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
            currentUser.Action = "DeleteAcctMngt";
            
            HandleState hs = accountingService.Delete(id);
            if (hs.Code == 403)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.DO_NOT_HAVE_PERMISSION].Value });
            }

            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            if (!hs.Success)
            {
                ResultHandle _result = new ResultHandle { Status = hs.Success, Message = hs.Message.ToString() };
                return BadRequest(_result);
            }
            else
            {
                Response.OnCompleted(async () =>
                {
                    List<ObjectReceivableModel> modelReceivableList = accountingService.CalculatorReceivableAcctMngt(id);
                    await accountReceivableService.InsertOrUpdateReceivableAsync(modelReceivableList);

                });
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
            currentUser.Action = "AddAcctMngt";

            if (!ModelState.IsValid) return BadRequest();

            //Check Exist Voucher ID of VAT Invoice
            var isExistedVoucherId = CheckExistedVoucherId(model.VoucherId, model.Id);
            if (isExistedVoucherId)
            {
                return BadRequest(new ResultHandle { Status = false, Message = "Voucher ID has been existed" });
            }

            //Check Duplicate Voucher ID (In year) of Voucher
            var isDuplicateVoucherId = CheckDuplicateVoucherIdInYear(model.VoucherId, model.Id);
            if (isDuplicateVoucherId)
            {
                return BadRequest(new ResultHandle { Status = false, Message = string.Format("Voucher ID {0} existed in {1}", model.VoucherId, DateTime.Now.Year) });
            }

            if (model.Type == AccountingConstants.ADVANCE_TYPE_INVOICE)
            {
                var isExistedInvoiceNoTempSerie = CheckExistedInvoiceNoTempSerie(model.InvoiceNoTempt, model.Serie, model.Id);
                if (isExistedInvoiceNoTempSerie)
                {
                    return BadRequest(new ResultHandle { Status = false, Message = "Invoice No (Tempt) - Seri has been existed", Data = 409 });
                }
            }

            if (model.Charges.Count == 0)
            {
                string accountType = model.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE ? "VAT Invoice" : "Voucher";
                ResultHandle _result = new ResultHandle { Status = false, Message = accountType + " don't have any charge in this period, Please check it again!" };
                return BadRequest(_result);
            }

            //Check tồn tại phí debit đã sync khi issue Vat Invoice
            var isExistDebitSynced = accountingService.CheckExistDebitChargeSynced(model.Charges, model.Type);
            if (isExistDebitSynced)
            {
                return BadRequest(new ResultHandle { Status = false, Message = "Existing charges have been synchronized to the accounting system, Please check it again!" });
            }
            
            var hs = accountingService.AddAcctMgnt(model);

            //if (hs.Success)
            //{
            //    // Tính công nợ dựa vào id của Accounting Management
            //    var cr = accountingService.CalculatorReceivableAcctMngt(model.Id);
            //}

            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = model };
            if (!hs.Success)
            {
                result.Data = null;
                return BadRequest(result);
            }
            else
            {
                Response.OnCompleted(async () =>
                {
                    List<ObjectReceivableModel> modelReceivableList = accountingService.CalculatorReceivableAcctMngt(model.Id);
                    await accountReceivableService.InsertOrUpdateReceivableAsync(modelReceivableList);

                });
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
            currentUser.Action = "UpdateAcctMngt";

            if (!ModelState.IsValid) return BadRequest();

            //Check Exist Voucher ID of VAT Invoice
            var isExistedVoucherId = CheckExistedVoucherId(model.VoucherId, model.Id);
            if (isExistedVoucherId)
            {
                return BadRequest(new ResultHandle { Status = false, Message = "Voucher ID has been existed" });
            }

            //Check Duplicate Voucher ID (In year) of Voucher
            var isDuplicateVoucherId = CheckDuplicateVoucherIdInYear(model.VoucherId, model.Id);
            if (isDuplicateVoucherId)
            {
                return BadRequest(new ResultHandle { Status = false, Message = string.Format("Voucher ID {0} existed in {1}", model.VoucherId, DateTime.Now.Year) });
            }

            if (model.Type == AccountingConstants.ADVANCE_TYPE_INVOICE)
            {
                var isExistedInvoiceNoTempSerie = CheckExistedInvoiceNoTempSerie(model.InvoiceNoTempt, model.Serie, model.Id);
                if (isExistedInvoiceNoTempSerie)
                {
                    return BadRequest(new ResultHandle { Status = false, Message = "Invoice No (Tempt) - Seri has been existed", Data = 409 });
                }
            }

            if (model.Charges.Count == 0)
            {
                string accountType = model.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE ? "VAT Invoice" : "Voucher";
                ResultHandle _result = new ResultHandle { Status = false, Message = accountType + " don't have any charge in this period, Please check it again!" };
                return BadRequest(_result);
            }

            //Check tồn tại phí đã Sync khi issue Vat Invoice
            var isExistDebitSynced = accountingService.CheckExistDebitChargeSynced(model.Charges, model.Type);
            if (isExistDebitSynced)
            {
                return BadRequest(new ResultHandle { Status = false, Message = "Existing charges have been synchronized to the accounting system, Please check it again!" });
            }

            var hs = accountingService.UpdateAcctMngt(model);

            //if (hs.Success)
            //{
            //    // Tính công nợ dựa vào id của Accounting Management
            //    accountingService.CalculatorReceivableAcctMngt(model.Id);
            //}

            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = model };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            else
            {
                Response.OnCompleted(async () =>
                {
                    List<ObjectReceivableModel> modelReceivableList = accountingService.CalculatorReceivableAcctMngt(model.Id);
                    await accountReceivableService.InsertOrUpdateReceivableAsync(modelReceivableList);

                });
            }
            return Ok(result);
        }

        /// <summary>
        /// Check Exist Voucher ID of VAT Invoice
        /// </summary>
        /// <param name="voucherId"></param>
        /// <param name="acctId"></param>
        /// <returns></returns>
        [HttpGet("CheckVoucherIdExist")]
        public IActionResult CheckVoucherIdExist(string voucherId, Guid? acctId)
        {
            var isExited = CheckExistedVoucherId(voucherId, acctId);
            return Ok(isExited);
        }

        /// <summary>
        /// Check Exist Voucher ID of VAT Invoice
        /// </summary>
        /// <param name="voucherId"></param>
        /// <param name="acctId"></param>
        /// <returns></returns>
        private bool CheckExistedVoucherId(string voucherId, Guid? acctId)
        {
            var isExited = false;
            if (acctId == Guid.Empty || acctId == null)
            {
                isExited = accountingService.Get(x => x.VoucherId == voucherId && x.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE).Any();
            }
            else
            {
                isExited = accountingService.Get(x => x.VoucherId == voucherId && x.Id != acctId && x.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE).Any();
            }
            return isExited;
        }

        [HttpGet("GenerateVoucherId")]
        public IActionResult GenerateVoucherId(string acctMngtType, string voucherType)
        {
            var voucherId = accountingService.GenerateVoucherId(acctMngtType, voucherType);
            return Ok(new { VoucherId = voucherId });
        }

        /// <summary>
        /// check permission of user to view a accounting management
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("CheckPermission/{id}")]
        [Authorize]
        public IActionResult CheckDetailPermission(Guid id)
        {
            var result = accountingService.CheckDetailPermission(id);
            if (result == 403) return Ok(false);
            return Ok(true);
        }

        [HttpGet("GetById")]
        [Authorize]
        public IActionResult GetDetailById(Guid id)
        {
            var statusCode = accountingService.CheckDetailPermission(id);
            if (statusCode == 403)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.DO_NOT_HAVE_PERMISSION].Value });
            }
            var detail = accountingService.GetAcctMngtById(id);
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
                isExited = accountingService.Get(x => x.InvoiceNoTempt == invoiceNoTemp && x.Serie == serie && x.Type == AccountingConstants.ADVANCE_TYPE_INVOICE).Any();
            }
            else
            {
                isExited = accountingService.Get(x => x.InvoiceNoTempt == invoiceNoTemp && x.Serie == serie && x.Id != acctId && x.Type == AccountingConstants.ADVANCE_TYPE_INVOICE).Any();
            }
            return isExited;
        }

        [HttpPost("GetDataAcctMngtExport")]
        [Authorize]
        public IActionResult GetDataAcctMngtExport(AccAccountingManagementCriteria criteria)
        {
            var data = accountingService.GetDataAcctMngtExport(criteria);
            return Ok(data);
        }

        [HttpGet("DownLoadVatInvoiceTemplate")]
        public async Task<ActionResult> DownLoadVatInvoiceTemplate()
        {
            var result = await new FileHelper().ExportExcel(_hostingEnvironment.ContentRootPath, "VatInvoiceImportTemplate.xlsx");
            if (result != null)
            {
                return result;
            }
            else
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.FILE_NOT_FOUND].Value });
            }
        }

        [HttpPost]
        [Route("uploadVatInvoiceImportTemplate")]
        public IActionResult uploadVatInvoiceImportTemplate(IFormFile uploadedFile)
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

                List<AcctMngtVatInvoiceImportModel> list = new List<AcctMngtVatInvoiceImportModel>();
                for (int row = 2; row <= rowCount; row++)
                {
                    string date = worksheet.Cells[row, 3].Value?.ToString().Trim();
                    DateTime? dateToPase = null;

                    if (date != null)
                    {
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
                    }

                    var acc = new AcctMngtVatInvoiceImportModel
                    {
                        IsValid = true,
                        VoucherId = worksheet.Cells[row, 1].Value?.ToString().Trim(),
                        RealInvoiceNo = worksheet.Cells[row, 2].Value?.ToString().Trim(),
                        InvoiceDate = !string.IsNullOrEmpty(date) ? dateToPase : (DateTime?)null,
                        SerieNo = worksheet.Cells[row, 4].Value?.ToString().Trim(),
                        PaymentStatus = worksheet.Cells[row, 5].Value?.ToString().Trim(),
                    };
                    list.Add(acc);
                }
                List<AcctMngtVatInvoiceImportModel> data = accountingService.CheckVatInvoiceImport(list);

                int totalValidRows = data.Count(x => x.IsValid == true);
                var results = new { data, totalValidRows };
                return Ok(results);
            }
            return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.FILE_NOT_FOUND].Value });
        }

        [HttpPut]
        [Route("ImportVatInvoice")]
        [Authorize]
        public IActionResult ImportVatInvoice(List<AcctMngtVatInvoiceImportModel> model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var result = accountingService.ImportVatInvoice(model);
            if (result.Status)
            {
                var acctMngtIds = accountingService.Get(x => x.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE && model.Select(s => s.VoucherId).Contains(x.VoucherId)).Select(s => s.Id);
                foreach(var acctMngtId in acctMngtIds)
                {
                    //accountingService.CalculatorReceivableAcctMngt(acctMngtId);
                    Response.OnCompleted(async () =>
                    {
                        List<ObjectReceivableModel> modelReceivableList = accountingService.CalculatorReceivableAcctMngt(acctMngtId);
                        await accountReceivableService.InsertOrUpdateReceivableAsync(modelReceivableList);

                    });
                }
            }
            return Ok(result);
        }

        [HttpPost]
        [Route("GetContractForInvoice")]
        [Authorize]
        public IActionResult GetContractForInvoice(AccMngtContractInvoiceCriteria model)
        {
            if (!ModelState.IsValid) return BadRequest();
            CatContractInvoiceModel result = accountingService.GetContractForInvoice(model);
            return Ok(result);
        }
        
        [HttpPost]
        [Route("CalculateListChargeAccountingMngt")]
        public IActionResult CalculateListChargeAccountingMngt(List<ChargeOfAccountingManagementModel> charges)
        {
            ChargeAccountingMngtTotalViewModel result = accountingService.CalculateListChargeAccountingMngt(charges);
            return Ok(result);
        }

        /// <summary>
        /// get and paging the list of Confirm Biling by conditions
        /// </summary>
        /// <param name="criteria">search conditions</param>
        /// <param name="pageNumber">page to retrieve data</param>
        /// <param name="pageSize">number items per page</param>
        /// <returns></returns>
        [HttpPost]
        [Route("ConfirmBillingPaging")]
        [Authorize]
        public IActionResult ConfirmBillingPaging(ConfirmBillingCriteria criteria, int pageNumber, int pageSize)
        {
            var data = accountingService.ConfirmBillingPaging(criteria, pageNumber, pageSize, out int totalItems);
            var result = new { data, totalItems, pageNumber, pageSize };
            return Ok(result);
        }

        /// <summary>
        /// Update Confirm Billing Date for Invoice
        /// </summary>
        /// <param name="ids">List is of Invoice</param>
        /// <param name="billingDate">Billing Date of Invoice</param>
        /// <returns></returns>
        [HttpPut]
        [Route("UpdateConfirmBillingDate")]
        [Authorize]
        public IActionResult UpdateConfirmBillingDate(List<Guid> ids, DateTime? billingDate)
        {
            var hs = accountingService.UpdateConfirmBillingDate(ids, billingDate);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = "Update confirm billing date successful!", Data = ids };
            if (!hs.Success)
            {
                ResultHandle _result = new ResultHandle { Status = hs.Success, Message = hs.Message.ToString() };
                return BadRequest(_result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Check duplicate Voucher ID of Voucher
        /// </summary>
        /// <param name="voucherId"></param>
        /// <param name="acctId"></param>
        /// <returns></returns>
        [HttpGet("CheckDuplicateVoucherId")]
        public IActionResult CheckDuplicateVoucherId(string voucherId, Guid? acctId)
        {
            var isDuplicate = CheckDuplicateVoucherIdInYear(voucherId, acctId);
            return Ok(isDuplicate);
        }

        /// <summary>
        /// Check duplicate Voucher ID (In Year) of Voucher
        /// </summary>
        /// <param name="voucherId"></param>
        /// <param name="acctId"></param>
        /// <returns></returns>
        private bool CheckDuplicateVoucherIdInYear(string voucherId, Guid? acctId)
        {
            var isDuplicated = false;
            int yearCurr = DateTime.Now.Year;
            //Case: Insert
            if (acctId == Guid.Empty || acctId == null)
            {
                isDuplicated = accountingService.Get(x => x.VoucherId == voucherId && x.Type == AccountingConstants.ACCOUNTING_VOUCHER_TYPE && (x.DatetimeCreated.HasValue ? x.DatetimeCreated.Value.Year : yearCurr) == yearCurr).Any();
            }
            //Case: Update
            else
            {
                isDuplicated = accountingService.Get(x => x.VoucherId == voucherId && x.Type == AccountingConstants.ACCOUNTING_VOUCHER_TYPE && x.Id != acctId && (x.DatetimeCreated.HasValue ? x.DatetimeCreated.Value.Year : yearCurr) == yearCurr).Any();
            }            
            return isDuplicated;
        }
    }
}
