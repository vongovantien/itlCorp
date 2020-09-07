using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.ForPartner.DL.Common;
using eFMS.API.ForPartner.DL.IService;
using eFMS.API.ForPartner.DL.Models;
using eFMS.API.ForPartner.DL.Models.Criteria;
using eFMS.API.ForPartner.Infrastructure.Filters;
using eFMS.API.ForPartner.Infrastructure.Middlewares;
using eFMS.API.ForPartner.Service.Models;
using ITL.NetCore.Connection.EF;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace eFMS.API.ForPartner.Controllers
{
    /// <summary>
    /// Accounting Controller
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [MiddlewareFilter(typeof(LocalizationMiddleware))]
    [Route("api/v{version:apiVersion}/{lang}/[controller]")]
    public class AccountingController : ControllerBase
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly IAccountingManagementService accountingManagementService;

        /// <summary>
        /// Accounting Contructor
        /// </summary>
        public AccountingController(IAccountingManagementService service)
        {
            accountingManagementService = service;
        }

        /// <summary>
        /// Test
        /// </summary>
        /// <remarks>
        /// Remark
        /// </remarks>
        /// <returns></returns>
        /// <response></response>
        [HttpGet("Test")]
        [APIKeyAuth]
        public IActionResult Test()
        {
            return Ok("OK");
        }


        [HttpGet("GetInvoice")]
        [APIKeyAuth]
        public IActionResult GetInvoice()
        {
            string apiKey = Request.Headers[AccountingConstants.API_KEY_HEADER];
            if (!accountingManagementService.ValidateApiKey(apiKey)){
                return Unauthorized();
            }
            AccAccountingManagementModel data = accountingManagementService.GetById(Guid.NewGuid());
            return Ok(data);
        }

        [HttpPost("GetChargeInvoice")]
        // [APIKeyAuth]
        public IActionResult GetChargeInvoice([FromBody]SearchAccMngtCriteria model)
        {
            //string apiKey = Request.Headers[AccountingConstants.API_KEY_HEADER];
            //if (!accountingManagementService.ValidateApiKey(apiKey))
            //{
            //    return Unauthorized();
            //}

            List<ChargeOfAcctMngtResult> data = accountingManagementService.GetChargeInvoice(model);
            return Ok(data);
        }

    }
}