using System;
using System.Collections.Generic;
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
        /// Calculator Receivable Not Authorize
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("CalculatorReceivableNotAuthorize")]
        public IActionResult CalculatorReceivableNotAuthorize(CalculatorReceivableNotAuthorizeModel model)
        {
            var calculatorReceivable = accountReceivableService.CalculatorReceivableNotAuthorize(model);
            return Ok(calculatorReceivable);
        }

        /// <summary>
        /// Insert Or Update Receivable
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        [HttpPost("InsertOrUpdateReceivable")]
        [Authorize]
        public IActionResult InsertOrUpdateReceivable(List<ObjectReceivableModel> models)
        {
            HandleState insertOrUpdateReceivable = accountReceivableService.InsertOrUpdateReceivable(models);
            var message = HandleError.GetMessage(insertOrUpdateReceivable, Crud.Update);
            if (insertOrUpdateReceivable.Success)
            {
                ResultHandle result = new ResultHandle { Status = insertOrUpdateReceivable.Success, Message = stringLocalizer[message].Value };
                return Ok(result);
            }
            return BadRequest(message);
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
        public IActionResult GetDetailAccountReceivableByPartnerId(string partnerId,string saleManId)
        {
            var data = accountReceivableService.GetDetailAccountReceivableByPartnerId(partnerId, saleManId);
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


        [HttpPost("GetDataARSumaryExport")]
        [Authorize]
        public IActionResult GetDataARSumaryExport(AccountReceivableCriteria criteria)
        {
            var data = accountReceivableService.GetDataARSumaryExport(criteria);
            return Ok(data);
        }

        [HttpPost("GetDebitDetail")]
        public IActionResult GetDebitDetail(AcctReceivableDebitDetailCriteria criteria)
        {
            var data = accountReceivableService.GetDataDebitDetail(criteria);
            return Ok(data);
        }

        [HttpPost("GetDebitDetailByPartnerId")]
        [Authorize]
        public IActionResult GetDebitDetailByPartnerId([FromBody]ArDebitDetailCriteria model)
        {
            var data = accountReceivableService.GetDebitDetailByPartnerId(model);
            return Ok(data);
        }
        /// <summary>
        /// Update due date invoice và công nợ quá hạn sau khi update HĐ
        /// </summary>
        /// <param name="contractModel"></param>
        /// <returns></returns>
        [HttpPost("UpdateDueDateAndOverDaysAfterChangePaymentTerm")]
        [Authorize]
        public IActionResult UpdateDueDateAndOverDaysAfterChangePaymentTerm(CatContractModel contractModel)
        {
            var result = accountReceivableService.UpdateDueDateAndOverDaysAfterChangePaymentTerm(contractModel);
            return Ok(result);
        }

        [HttpPut("CalculateOverDue1To15")]
        public IActionResult CalculateOverDue1To15([FromBody] List<string> partnerIds)
        {
            var hs = accountReceivableService.CalculatorReceivableOverDue1To15Day(partnerIds, out List<Guid?> contractIdstoUpdate);

            var message = HandleError.GetMessage(hs, Crud.Update);
            if (hs.Success)
            {
                ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
                Response.OnCompleted(async () =>
                {
                    await accountReceivableService.CalculateAgreementFlag(contractIdstoUpdate, "overdue");
                });

                return Ok(result);
            }
            return BadRequest(message);
        }

        [HttpPut("CalculateOverDue15To30")]
        public IActionResult CalculateOverDue15To30([FromBody] List<string> partnerIds)
        {
            var hs = accountReceivableService.CalculatorReceivableOverDue15To30Day(partnerIds, out List<Guid?> contractIdstoUpdate);

            var message = HandleError.GetMessage(hs, Crud.Update);
            if (hs.Success)
            {
                ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
                Response.OnCompleted(async () =>
                {
                    await accountReceivableService.CalculateAgreementFlag(contractIdstoUpdate, "overdue");
                });
                return Ok(result);
            }
            return BadRequest(message);
        }

        [HttpPut("CalculateOverDue30")]
        public IActionResult CalculateOverDue30([FromBody] List<string> partnerIds)
        {
            var hs = accountReceivableService.CalculatorReceivableOverDue30Day(partnerIds, out List<Guid?> contractIdstoUpdate);

            var message = HandleError.GetMessage(hs, Crud.Update);
            if (hs.Success)
            {
                ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
                Response.OnCompleted(async () =>
                {
                    await accountReceivableService.CalculateAgreementFlag(contractIdstoUpdate, "overdue");
                });
                return Ok(result);
            }

         
            return BadRequest(message);
        }

        [HttpPut("CalculateDebitAmount")]
        public async Task<IActionResult> CalculateDebitAmount(List<ObjectReceivableModel> models)
        {
            var hs = await accountReceivableService.CalculatorReceivableDebitAmountAsync(models);

            var message = HandleError.GetMessage(hs, Crud.Update);
            if (hs.Success)
            {
                ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
                return Ok(result);
            }
            return BadRequest(message);
        }
    }
}