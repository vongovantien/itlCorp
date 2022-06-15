﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models.AccountingPayable;
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
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPost("GetBy")]
        public IActionResult GetBy(AcctPayableViewDetailCriteria criteria)
        {
            var results = acctPayableService.GetBy(criteria);
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

        /// <summary>
        /// Get data export accounting template payable
        /// </summary>
        /// <param name="partnerId"></param>
        /// <returns></returns>
        [HttpGet("GetGeneralPayable")]
        [Authorize]
        public IActionResult GetGeneralPayable(string partnerId)
        {
            var data = acctPayableService.GetGeneralPayable(partnerId);
            return Ok(data);
        }

        /// <summary>
        /// Get data export accounting template payable
        /// </summary>
        /// <param name="partnerId"></param>
        /// <param name="paymentTerm"></param>
        /// <param name="currency"></param>
        /// <returns></returns>
        [HttpGet("UpdatePayable")]
        [Authorize]
        public IActionResult UpdatePayable(string partnerId, int paymentTerm, string currency)
        {
            var data = acctPayableService.UpdatePayable(partnerId, paymentTerm, currency);
            return Ok(data);
        }
    }
}