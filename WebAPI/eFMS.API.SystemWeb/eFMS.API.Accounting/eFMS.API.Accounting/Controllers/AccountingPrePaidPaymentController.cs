using System;
using System.Collections.Generic;
using System.Linq;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.Infrastructure.Middlewares;
using eFMS.API.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eFMS.API.Accounting.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class AccountingPrePaidPaymentController : ControllerBase
    {
        private readonly IAccountingPrePaidPaymentService prepaidService;
        public AccountingPrePaidPaymentController(IAccountingPrePaidPaymentService prepaidService)
        {
            this.prepaidService = prepaidService;
        }

        [HttpPost]
        [Route("Paging")]
        [Authorize]
        public IActionResult Paging(AccountingPrePaidPaymentCriteria criteria, int page, int size)
        {
            IQueryable<AccPrePaidPaymentResult> data = prepaidService.Paging(criteria, page, size, out int rowsCount);
            var result = new ResponsePagingModel<AccPrePaidPaymentResult> { Data = data, Page = page, Size = size, TotalItems = rowsCount };
            return Ok(result);
        }
    }
}