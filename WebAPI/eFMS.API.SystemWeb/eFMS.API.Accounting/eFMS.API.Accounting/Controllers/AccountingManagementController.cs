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
using System.Collections.Generic;
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
    public class AccountingManagementController: ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly IAccountingManagementService accountingService;
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
            
            var hs = accountingService.UpdateAcctMngt(model);
            
            var message = HandleError.GetMessage(hs, Crud.Update);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = model };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
    }
}
