﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.AccountReceivable;
using eFMS.API.Accounting.DL.Models.Criteria;
using eFMS.API.Accounting.Infrastructure.Middlewares;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.API.Infrastructure.RabbitMQ;
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
        private readonly IRabbitBus _busControl;
        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="localizer"></param>
        /// <param name="accountReceivable"></param>
        public AccountReceivableController(IStringLocalizer<LanguageSub> localizer,
            IRabbitBus busControl,
            IAccAccountReceivableService accountReceivable)
        {
            stringLocalizer = localizer;
            accountReceivableService = accountReceivable;
            _busControl = busControl;
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
        /// Get AR detail has argeement by argeement id
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPost("GetDebitAmountDetailByContract")]
        public IActionResult GetDebitAmountDetailByContract(AccAccountReceivableCriteria criteria)
        {
            var data = accountReceivableService.GetDebitAmountDetailByContract(criteria);
            return Ok(data);
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
        public async Task<IActionResult> UpdateDueDateAndOverDaysAfterChangePaymentTerm(CatContractModel contractModel)
        {
            var result = await accountReceivableService.UpdateDueDateAndOverDaysAfterChangePaymentTerm(contractModel);
            List<string> partnerIds = new List<string> { contractModel.PartnerId };
          
            Response.OnCompleted(async () =>
            {
                CalculateOverDue1To15(partnerIds);
                CalculateOverDue15To30(partnerIds);
                CalculateOverDue30(partnerIds);
            });
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
            // var hs = await accountReceivableService.CalculatorReceivableDebitAmountAsync(models);
            //var message = HandleError.GetMessage(hs, Crud.Update);
            //if (hs.Success)
            //{
            //    ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value };
            //    return Ok(result);
            //}
            //return BadRequest(message);

            await _busControl.SendAsync(RabbitConstants.CalculatingReceivableDataPartnerQueue, models);
            ResultHandle result = new ResultHandle { Status = true, Message = stringLocalizer[LanguageSub.MSG_UPDATE_SUCCESS].Value };
            return Ok(result);
        }

        [HttpPut("MoveSalesmanReceivableData")]
        [Authorize]
        public async Task<IActionResult> MoveSalesmanReceivableData(AccountReceivableMoveDataSalesman model)
        {
            var hs = await accountReceivableService.MoveReceivableData(model);

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