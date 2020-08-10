using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using eFMS.API.Accounting.DL.Common;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.AccountingPayment;
using eFMS.API.Accounting.DL.Models.Criteria;
using eFMS.API.Accounting.Infrastructure.Middlewares;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Helpers;
using eFMS.API.Common.Infrastructure.Common;
using ExcelDataReader;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OfficeOpenXml;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace eFMS.API.Accounting.Controllers
{
    /// <summary>
    /// A base class for an MVC controller without view support.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class AccountingPaymentController : Controller
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly IAccAccountingPaymentService accountingPaymentService;
        private readonly IHostingEnvironment _hostingEnvironment;
        

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="paymentService"></param>
        /// <param name="hostingEnvironment"></param>
        public AccountingPaymentController(IStringLocalizer<LanguageSub> localizer,
            IAccAccountingPaymentService paymentService,
            IHostingEnvironment hostingEnvironment)
        {
            stringLocalizer = localizer;
            accountingPaymentService = paymentService;
            _hostingEnvironment = hostingEnvironment;
            
        }
        
        /// <summary>
        /// query and paging VAT invoice / SOA
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpPost("Paging")]
        [Authorize]
        public IActionResult PagingPayment(PaymentCriteria criteria, int pageNumber, int pageSize)
        {
            var data = accountingPaymentService.Paging(criteria, pageNumber, pageSize, out int totalItems);
            var result = new { data, totalItems, pageNumber, pageSize };
            return Ok(result);
        }
        /// <summary>
        /// get list payment by refNo
        /// </summary>
        /// <param name="refId"></param>
        /// <returns></returns>
        [HttpGet("GetBy")]
        public IActionResult GetBy(string refId)
        {
            var results = accountingPaymentService.GetBy(refId);
            return Ok(results);
        }
        /// <summary>
        /// download file excel from server
        /// </summary>
        /// <returns></returns>
        [HttpGet("DownloadInvoicePaymentExcel")]
        public async Task<ActionResult> DownloadInvoicePaymentExcel()
        {
            string fileName = Templates.AccountingPayment.ExelInvoicePaymentImportFileName + Templates.ExelImportEx;
            string templateName = _hostingEnvironment.ContentRootPath;
            var result = await new FileHelper().ExportExcel(templateName, fileName);
            if (result != null)
            {
                return result;
            }
            return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.FILE_NOT_FOUND].Value });
        }

        /// <summary>
        /// download file excel from server
        /// </summary>
        /// <returns></returns>
        [HttpGet("DownloadOBHPaymentExcel")]
        public async Task<ActionResult> DownloadOBHPaymentExcel()
        {
            string fileName = Templates.AccountingPayment.ExelOBHPaymentImportFileName + Templates.ExelImportEx;
            string templateName = _hostingEnvironment.ContentRootPath;
            var result = await new FileHelper().ExportExcel(templateName, fileName);
            if (result != null)
            {
                return result;
            }
            return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.FILE_NOT_FOUND].Value });
        }
        /// <summary>
        /// read commodities data from file excel
        /// </summary>
        /// <param name="uploadedFile">file to read data</param>
        /// <returns></returns>
        [HttpPost]
        [Route("UploadInvoicePaymentFile")]
        public IActionResult UploadFile(IFormFile uploadedFile)
        {
            var file = new FileHelper().UploadExcel(uploadedFile);
            if (file != null)
            {
                int totalValidRows = 0;

                ExcelWorksheet worksheet = file.Workbook.Worksheets[1];
                int rowCount = worksheet.Dimension.Rows;
                int colCount = worksheet.Dimension.Columns;
                if (rowCount < 2) return BadRequest();

                List<AccountingPaymentImportModel> list = new List<AccountingPaymentImportModel>();
                list = ReadInvoicePaymentData(worksheet, rowCount);
                if(list.Any(x => x.IsValid == false))
                {
                    totalValidRows = list.Count(x => x.IsValid == true);
                    return Ok(new { data = list, totalValidRows });
                }
                List<AccountingPaymentImportModel> data = accountingPaymentService.CheckValidImportInvoicePayment(list);
                if (data != null)
                {
                    totalValidRows = data.Count(x => x.IsValid == true);
                }
                var results = new { data, totalValidRows };
                return Ok(results);

            }
            return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.FILE_NOT_FOUND].Value });
        }

        private List<AccountingPaymentImportModel> ReadInvoicePaymentData(ExcelWorksheet worksheet, int rowCount)
        {
            List<AccountingPaymentImportModel> list = new List<AccountingPaymentImportModel>();
            for (int row = 2; row <= rowCount; row++)
            {
                string invoiceNo = worksheet.Cells[row, 1].Value?.ToString();
                var payment = new AccountingPaymentImportModel
                {
                    IsValid = true,
                    PartnerName = worksheet.Cells[row, 4].Value?.ToString(),
                    Note = worksheet.Cells[row, 8].Value?.ToString()
                };
                if (worksheet.Cells[row, 1].Value == null)
                {
                    payment.InvoiceNoError = stringLocalizer[AccountingLanguageSub.MSG_INVOICENO_ACCOUNTING_PAYMENT_EMPTY].Value;
                    payment.IsValid = false;
                }
                else
                {
                    payment.InvoiceNo = worksheet.Cells[row, 1].Value.ToString();
                }
                if (worksheet.Cells[row, 2].Value == null)
                {
                    payment.SerieNoError = stringLocalizer[AccountingLanguageSub.MSG_SERIENO_ACCOUNTING_PAYMENT_EMPTY].Value;
                    payment.IsValid = false;
                }
                else
                {
                    payment.SerieNo = worksheet.Cells[row, 2].Value.ToString();
                }
                if (worksheet.Cells[row, 3].Value == null)
                {
                    payment.PartnerAccountError = stringLocalizer[AccountingLanguageSub.MSG_PARTNER_ACCOUNTING_PAYMENT_EMPTY].Value;
                    payment.IsValid = false;
                }
                else
                {
                    payment.PartnerAccount = worksheet.Cells[row, 3].Value.ToString();
                }
                if (worksheet.Cells[row, 5].Value == null)
                {
                    payment.PaymentAmountError = stringLocalizer[AccountingLanguageSub.MSG_PAYMENT_AMOUNT_ACCOUNTING_PAYMENT_EMPTY].Value;
                    payment.IsValid = false;
                }
                else
                {
                    var checkPaymentAmount = decimal.TryParse(worksheet.Cells[row, 5].Value.ToString(), out decimal paymentAmount);
                    if (checkPaymentAmount == false)
                    {
                        payment.PaymentAmountError = stringLocalizer[AccountingLanguageSub.MSG_PAYMENT_AMOUNT_ACCOUNTING_PAYMENT_MUST_BE_NUMBER].Value;
                        payment.IsValid = false;
                    }
                    else
                    {
                        payment.PaymentAmount = paymentAmount;
                    }
                }
                if (worksheet.Cells[row, 6].Value == null){
                    payment.CurrencyIdError = stringLocalizer[AccountingLanguageSub.MSG_CURRENCY_ACCOUNTING_PAYMENT_EMPTY].Value;
                    payment.IsValid = false;
                }
                else
                {
                    payment.CurrencyId = worksheet.Cells[row, 6].Value.ToString();
                }
                if (worksheet.Cells[row, 7].Value != null)
                {
                    var checkExchangeRate = decimal.TryParse(worksheet.Cells[row, 7].Value.ToString(), out decimal exchangeRate);
                    if (!checkExchangeRate)
                    {
                        payment.ExchangeRateError = stringLocalizer[AccountingLanguageSub.MSG_EXCHANGE_RATE_ACCOUNTING_PAYMENT_INVALID, worksheet.Cells[row,7].Value.ToString()].Value;
                        payment.IsValid = false;
                    }
                    else
                    {
                        payment.ExchangeRate = exchangeRate;
                    }
                }
                if(worksheet.Cells[row,8].Value == null)
                {
                    payment.IsValid = false;
                    payment.PaymentMethodError = stringLocalizer[AccountingLanguageSub.MSG_PAYMENT_METHOD_ACCOUNTING_PAYMENT_EMPTY].Value;
                }
                else
                {
                    string paymentMethod = worksheet.Cells[row, 8].Value.ToString();
                    if (paymentMethod.ToLower() == "cash" || paymentMethod.ToLower() == "bank transfer")
                    {
                        payment.PaymentMethod = worksheet.Cells[row, 8].Value.ToString();
                    }
                    else
                    {
                        payment.PaymentMethodError = stringLocalizer[AccountingLanguageSub.MSG_PAYMENT_METHOD_ACCOUNTING_PAYMENT_INVALID, worksheet.Cells[row, 8].Value.ToString()].Value;
                        payment.IsValid = false;
                    }
                }
                if (worksheet.Cells[row, 9].Value == null)
                {
                    payment.PaidDateError = stringLocalizer[AccountingLanguageSub.MSG_PAIDDATE_ACCOUNTING_PAYMENT_EMPTY].Value;
                    payment.IsValid = false;
                }
                else
                {
                    if (DateTime.TryParse(worksheet.Cells[row, 9].Value.ToString(), out DateTime dDate))
                    {
                        payment.PaidDate = dDate;
                    }
                    else
                    {
                        payment.PaidDateError = stringLocalizer[AccountingLanguageSub.MSG_PAIDDATE_ACCOUNTING_PAYMENT_INVALID].Value;
                        payment.IsValid = false;
                    }
                }
                if (worksheet.Cells[row, 10].Value == null)
                {
                    payment.PaymentTypeError = stringLocalizer[AccountingLanguageSub.MSG_PAYMENT_TYPE_ACCOUNTING_PAYMENT_EMPTY].Value;
                    payment.IsValid = false;
                }
                else
                {
                    string paymentType = worksheet.Cells[row, 10].Value.ToString();
                    if (paymentType == "Net Off" || paymentType == "Normal")
                    {
                        payment.PaymentType = worksheet.Cells[row, 10].Value.ToString();
                    }
                    else
                    {
                        payment.PaymentTypeError = stringLocalizer[AccountingLanguageSub.MSG_PAYMENT_TYPE_ACCOUNTING_PAYMENT_INVALID].Value;
                        payment.IsValid = false;
                    }
                }

                payment.Note = worksheet.Cells[row, 11].Value.ToString();

                list.Add(payment);
            }
            return list;
        }

        /// <summary>
        /// import payments for invoice
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("ImportInvoicePayment")]
        public IActionResult ImportInvoicePayment([FromBody]List<AccountingPaymentImportModel> list)
        {
            var hs = accountingPaymentService.ImportInvoicePayment(list);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = "Import successfully !!!" };
            if (!hs.Success)
            {
                return BadRequest(new ResultHandle { Status = false, Message = hs.Message.ToString() });
            }
            return Ok(result);
        }
        /// <summary>
        /// import payments for SOA(type charge is OBH)
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("ImportSOAOBHPayment")]
        public IActionResult ImportSOAOBHPayment([FromBody]List<AccountingPaymentOBHImportTemplateModel> list)
        {
            var hs = accountingPaymentService.ImportOBHPayment(list);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = "Import successfully !!!" };
            if (!hs.Success)
            {
                return BadRequest(new ResultHandle { Status = false, Message = hs.Message.ToString() });
            }
            return Ok(result);
        }

        /// <summary>
        /// update extend date
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPut("UpdateExtendDate")]
        public IActionResult UpdateExtendDate(ExtendDateUpdatedModel model)
        {
            var hs = accountingPaymentService.UpdateExtendDate(model);
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
        /// Delete a payment
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            var hs = accountingPaymentService.Delete(id);
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
        /// get extended date of an invoice
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("GetInvoiceExtendedDate")]
        public IActionResult GetInvoiceExtendedDate(string id)
        {
            var result = accountingPaymentService.GetInvoiceExtendedDate(id);
            return Ok(result);
        }


        /// <summary>
        /// get extended date of an SOA(charge type = OBH)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("GetOBHSOAExtendedDate")]
        public IActionResult GetOBHSOAExtendedDate(string id)
        {
            var result = accountingPaymentService.GetOBHSOAExtendedDate(id);
            return Ok(result);
        }

        /// <summary>
        /// upload OBH payment template
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost("UploadOBHPaymentFile")]
        public IActionResult UploadOBHPaymentFile(IFormFile file)
        {
            //read data
            List<AccountingPaymentOBHImportTemplateModel> dataList = null;
            int totalRows = 0;
            int totalValidRows = 0;
            using (ExcelPackage package = new ExcelPackage(file.OpenReadStream()))
            {
                ExcelWorksheet workSheet = package.Workbook.Worksheets[1];
                totalRows = workSheet.Dimension.Rows;
                dataList = ReadOBHPaymentFile(workSheet, totalRows);
            }

            if (dataList.Any(x => x.IsValid == false))
            {
                totalValidRows = dataList.Count(x => x.IsValid == true);
                return Ok(new { data = dataList, totalValidRows });
            }
            List<AccountingPaymentOBHImportTemplateModel> data = accountingPaymentService.CheckValidImportOBHPayment(dataList);
            if (data != null)
            {
                totalValidRows = data.Count(x => x.IsValid == true);
            }
            return Ok(new { totalValidRows, data});
        }

        private List<AccountingPaymentOBHImportTemplateModel> ReadOBHPaymentFile(ExcelWorksheet workSheet, int totalRows)
        {
            var dataList = new List<AccountingPaymentOBHImportTemplateModel>();
            for (int i = 2; i <= totalRows; i++)
            {
                AccountingPaymentOBHImportTemplateModel data = new AccountingPaymentOBHImportTemplateModel
                {
                    // gán true trước sau đó lỗi gán lại false
                    IsValid = true
                };
                //
                if (
                    (workSheet.Cells[i, 1].Value == null || workSheet.Cells[i, 1].Value.ToString().Trim() == "") ||
                    (workSheet.Cells[i, 2].Value == null || workSheet.Cells[i, 2].Value.ToString().Trim() == "") ||
                    (workSheet.Cells[i, 4].Value == null || workSheet.Cells[i, 4].Value.ToString().Trim() == "" ||
                    (workSheet.Cells[i, 5].Value == null || workSheet.Cells[i, 5].Value.ToString().Trim() == "") ||
                    !Int32.TryParse(workSheet.Cells[i, 4].Value.ToString().Trim(), out int resultIntCheck)) ||
                    (workSheet.Cells[i, 8].Value == null || workSheet.Cells[i, 8].Value.ToString().Trim() == "") || 
                    !DateTime.TryParse(workSheet.Cells[i, 8].Value.ToString().Trim(), out DateTime resultDateCheck))
                {
                    data.IsValid = false;
                }
                data.SoaNo = workSheet.Cells[i, 1].Value == null ||                 // NULL
                    workSheet.Cells[i, 1].Value.ToString().Trim() == "" ?           // White space
                    null : workSheet.Cells[i, 1].Value.ToString();

                data.PartnerId = workSheet.Cells[i, 2].Value == null ||
                    workSheet.Cells[i, 2].Value.ToString().Trim() == "" ?
                    null : workSheet.Cells[i, 2].Value.ToString().Trim();

                data.PartnerName = workSheet.Cells[i, 3].Value == null ||
                    workSheet.Cells[i, 3].Value.ToString().Trim() == "" ?
                    null : workSheet.Cells[i, 3].Value.ToString().Trim();

                data.PaymentAmount = workSheet.Cells[i, 4].Value == null ||
                    workSheet.Cells[i, 4].Value.ToString().Trim() == "" ||
                    !Int32.TryParse(workSheet.Cells[i, 4].Value.ToString().Trim(), out int resultInt) ? // Type field invalid
                    (int?)null : int.Parse(workSheet.Cells[i, 4].Value.ToString().Trim());

                data.CurrencyId = workSheet.Cells[i, 5].Value == null ||
                    workSheet.Cells[i, 5].Value.ToString().Trim() == "" ?
                    null : workSheet.Cells[i, 5].Value.ToString().Trim();

                if (workSheet.Cells[i, 6].Value != null)
                {
                    bool checkExchangeRate = decimal.TryParse(workSheet.Cells[i, 6].Value.ToString(), out decimal exchangeRate);
                    if (!checkExchangeRate)
                    {
                        data.IsValid = false;
                    }
                    else
                    {
                        data.ExchangeRate = exchangeRate;
                    }
                }

                data.PaymentMethod = workSheet.Cells[i, 7].Value.ToString();

                if(!string.IsNullOrEmpty(data.PaymentMethod) && (data.PaymentMethod.ToLower() != "cash" && data.PaymentMethod.ToLower() != "bank transfer" ))
                {
                    data.IsValid = false;
                    data.PaymentMethodError = stringLocalizer[AccountingLanguageSub.MSG_PAYMENT_METHOD_ACCOUNTING_PAYMENT_INVALID, data.PaymentMethod].Value;
                }
                data.PaidDate = workSheet.Cells[i, 8].Value == null ||
                    workSheet.Cells[i, 8].Value.ToString().Trim() == "" ||
                    !DateTime.TryParse(workSheet.Cells[i, 8].Value.ToString().Trim(), out DateTime resultDate) ? // Type field invalid
                    (DateTime?)null : DateTime.Parse(workSheet.Cells[i, 8].Value.ToString().Trim());

                data.PaymentType = workSheet.Cells[i, 9].Value == null ||
                    workSheet.Cells[i, 9].Value.ToString().Trim() == "" ?
                    null : workSheet.Cells[i, 9].Value.ToString().Trim();

                data.Note = workSheet.Cells[i, 10].Value == null ||
                    workSheet.Cells[i, 10].Value.ToString().Trim() == "" ?
                    null : workSheet.Cells[i, 10].Value.ToString().Trim();

                dataList.Add(data);
            }
            return dataList;
        }
        [HttpPost("ExportAccountingPayment")]
        [Authorize]
        public IActionResult ExportAccountingPayment(PaymentCriteria criteria)
        {
            var data = accountingPaymentService.ExportAccountingPayment(criteria);
            return Ok(data);
        }
    }
}
