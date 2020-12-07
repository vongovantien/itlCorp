using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.Criteria;
using eFMS.API.Accounting.DL.Models.Receipt;
using eFMS.API.Accounting.Infrastructure.Middlewares;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
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
    }
}