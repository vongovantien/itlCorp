using System;
using System.Collections.Generic;
using System.Linq;
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
        public IActionResult PagingPayment(PaymentCriteria criteria, int pageNumber, int pageSize)
        {
            var data = accountingPaymentService.Paging(criteria, pageNumber, pageSize, out int totalItems);
            var result = new { data, totalItems, pageNumber, pageSize };
            return Ok(result);
        }
        /// <summary>
        /// get list payment by refNo
        /// </summary>
        /// <param name="refNo"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetBy(string refNo)
        {
            var results = accountingPaymentService.GetBy(refNo);
            return Ok(results);
        }
        /// <summary>
        /// download file excel from server
        /// </summary>
        /// <returns></returns>
        [HttpGet("DownloadInvoicePaymentExcel")]
        public async Task<ActionResult> DownloadExcel()
        {
            string fileName = Templates.AccountingPayment.ExelImportFileName + Templates.ExelImportEx;
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
                ExcelWorksheet worksheet = file.Workbook.Worksheets[1];
                int rowCount = worksheet.Dimension.Rows;
                int colCount = worksheet.Dimension.Columns;
                if (rowCount < 2) return BadRequest();
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
                    if(worksheet.Cells[row, 1].Value == null)
                    {
                        payment.InvoiceNoError = "Invoice No không được để trống";
                        payment.IsValid = false;
                    }
                    else
                    {
                        payment.InvoiceNo = worksheet.Cells[row, 1].Value.ToString();
                    }
                    if(worksheet.Cells[row, 2].Value == null)
                    {
                        payment.SerieNoError = "Serie No không được để trống";
                        payment.IsValid = false;
                    }
                    else
                    {
                        payment.SerieNo = worksheet.Cells[row, 2].Value.ToString();
                    }
                    if(worksheet.Cells[row, 3].Value == null)
                    {
                        payment.PartnerAccountError = "ParnerId không được để trống";
                        payment.IsValid = false;
                    }
                    else
                    {
                        payment.PartnerAccount = worksheet.Cells[row, 3].Value.ToString();
                    }
                    if(worksheet.Cells[row, 5].Value == null)
                    {
                        payment.PaymentAmountError = "Payment Amount không được để trống";
                        payment.IsValid = false;
                    }
                    else
                    {
                        var checkPaymentAmount = decimal.TryParse(worksheet.Cells[row, 5].Value.ToString(), out decimal paymentAmount);
                        if(checkPaymentAmount == false)
                        {
                            payment.PaymentAmountError = "Payment Amount phải là kiểu số";
                            payment.IsValid = false;
                        }
                        else
                        {
                            payment.PaymentAmount = paymentAmount;
                        }
                    }
                    if(worksheet.Cells[row, 6].Value == null)
                    {
                        payment.PaidDateError = "Paid Date không được để trống";
                        payment.IsValid = false;
                    }
                    else
                    {
                        if (DateTime.TryParse(worksheet.Cells[row, 6].Value.ToString(), out DateTime dDate))
                        {
                            payment.PaidDate = dDate;
                        }
                        else
                        {
                            payment.PaidDateError = "Paid Date không hợp lệ";
                            payment.IsValid = false;
                        }
                    }
                    if(worksheet.Cells[row, 7].Value == null)
                    {
                        payment.PaymentTypeError = "Payment Type không được để trống";
                        payment.IsValid = false;
                    }
                    else
                    {
                        string paymentType = worksheet.Cells[row, 7].Value.ToString();
                        if(paymentType == "Net Off" || paymentType == "Normal")
                        {
                            payment.PaymentType = worksheet.Cells[row, 7].Value.ToString();
                        }
                        else
                        {
                            payment.PaymentTypeError = "Payment Type không hợp lệ";
                            payment.IsValid = false;
                        }
                    }
                    list.Add(payment);
                }
                var data = accountingPaymentService.CheckValidImportInvoicePayment(list);
                var totalValidRows = data.Count(x => x.IsValid == true);
                var results = new { data, totalValidRows };
                return Ok(results);

            }
            return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[LanguageSub.FILE_NOT_FOUND].Value });
        }

        // POST api/<controller>
        [HttpPost]
        public void Import([FromBody]List<AccountingPaymentImportModel> list)
        {
        }
        
        /// <summary>
        /// update extend date
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
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
    }
}
