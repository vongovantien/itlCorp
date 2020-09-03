using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.Criteria;
using eFMS.API.Accounting.Infrastructure.Middlewares;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Infrastructure.Common;
using ITL.NetCore.Common;
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
    public class AccountReceivableController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly IAccAccountReceivableService accountReceivableService;

        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="accountReceivable"></param>
        public AccountReceivableController(IStringLocalizer<LanguageSub> localizer,
            IAccAccountReceivableService accountReceivable)
        {
            stringLocalizer = localizer;
            accountReceivableService = accountReceivable;
        }

        /// <summary>
        /// Get All Account Receivable
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetAll")]
        public IActionResult GetAll()
        {
            return Ok(accountReceivableService.Get());
        }

        /// <summary>
        /// Calculator Receivable
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("CalculatorReceivable")]
        [Authorize]
        public IActionResult CalculatorReceivable(CalculatorReceivableModel model)
        {
            var calculatorReceivable = accountReceivableService.CalculatorReceivable(model);
            return Ok(calculatorReceivable);
        }

        /// <summary>
        /// Insert Or Update Receivable
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("InsertOrUpdateReceivable")]
        [Authorize]
        public IActionResult InsertOrUpdateReceivable(ObjectReceivableModel model)
        {
            var insertOrUpdateReceivable = accountReceivableService.InsertOrUpdateReceivable(model);
            return Ok(insertOrUpdateReceivable);
        }

        /// <summary>
        /// Get AR detail has argeement by argeement id
        /// </summary>
        /// <param name="argeementId"></param>
        /// <returns></returns>
        [HttpGet("GetDetailAccountReceivableByArgeementId")]
        public IActionResult GetDetailAccountReceivableByArgeementId(Guid argeementId)
        {
            var data = accountReceivableService.GetDetailAccountReceivableByArgeementId(argeementId);
            return Ok(data);
        }

        /// <summary>
        /// Get AR detail no argeement by partner id
        /// </summary>
        /// <param name="partnerId"></param>
        /// <returns></returns>
        [HttpGet("GetDetailAccountReceivableByPartnerId")]
        public IActionResult GetDetailAccountReceivableByPartnerId(string partnerId)
        {
            var data = accountReceivableService.GetDetailAccountReceivableByPartnerId(partnerId);
            return Ok(data);
        }

        /// <summary>
        /// Paging
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Paging")]
        [Authorize]
        public IActionResult Paging(AccountReceivableCriteria criteria, int pageNumber, int pageSize)
        {
            var data = accountReceivableService.Paging(criteria, pageNumber, pageSize, out int totalItems);
            var result = new { data, totalItems, pageNumber, pageSize };
            return Ok(result);
        }

        /// <summary>
        /// Query Data
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPost("QueryData")]
        [Authorize]
        public IActionResult QueryData(AccountReceivableCriteria criteria)
        {
            var data = accountReceivableService.GetDataARByCriteria(criteria);
            return Ok(data);
        }
    }
}