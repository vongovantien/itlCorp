using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models.Criteria;
using eFMS.API.Accounting.Infrastructure.Middlewares;
using eFMS.API.Common.Globals;
using eFMS.IdentityServer.DL.UserManager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace eFMS.API.Accounting.Controllers
{
    /// <summary>
    /// A base class for an MVC controller without view support.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class AcctPayableController : Controller
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly IAcctPayableService acctPayableService;
        private readonly ICurrentUser currentUser;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="curUser"></param>
        /// <param name="acctPayable"></param>
        public AcctPayableController(IStringLocalizer<LanguageSub> localizer,
            ICurrentUser curUser,
           IAcctPayableService acctPayable)
        {
            stringLocalizer = localizer;
            acctPayableService = acctPayable;
            currentUser = curUser;
        }

        /// <summary>
        /// Load list pagine payable detail
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpPost("Paging")]
        [Authorize]
        public IActionResult Paging(AccountPayableCriteria criteria, int pageNumber, int pageSize)
        {
            var data = acctPayableService.Paging(criteria, pageNumber, pageSize, out int totalItems);
            var result = new { data, totalItems, pageNumber, pageSize };
            return Ok(result);
        }

        /// <summary>
        /// Get payment detail of payable transaction
        /// </summary>
        /// <param name="refNo"></param>
        /// <param name="type"></param>
        /// <param name="invoiceNo"></param>
        /// <param name="billingNo"></param>
        /// <returns></returns>
        [HttpGet("GetBy")]
        public IActionResult GetBy(string refNo, string type, string invoiceNo, string billingNo)
        {
            var results = acctPayableService.GetBy(refNo, type, invoiceNo, billingNo);
            return Ok(results);
        }

        /// <summary>
        /// Get data export Accounting Payable Standart
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPost("GetDataExportAccountingPayable")]
        [Authorize]
        public IActionResult GetDataExportAccountingPayable(AccountPayableCriteria criteria)
        {
            var data = acctPayableService.GetDataExportPayablePaymentDetail(criteria);
            return Ok(data);
        }

        /// <summary>
        /// Get data export accounting template payable
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPost("GetDataExportAccountingTemplate")]
        [Authorize]
        public IActionResult GetDataExportAccountingTemplate(AccountPayableCriteria criteria)
        {
            var data = acctPayableService.GetDataExportAccountingTemplate(criteria);
            return Ok(data);
        }
    }
}