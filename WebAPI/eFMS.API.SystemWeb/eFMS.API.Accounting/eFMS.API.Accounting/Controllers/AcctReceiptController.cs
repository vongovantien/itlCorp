using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models.Receipt;
using eFMS.API.Accounting.Infrastructure.Middlewares;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;

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
    }
}