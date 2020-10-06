using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.API.ForPartner.DL.Common;
using eFMS.API.ForPartner.DL.IService;
using eFMS.API.ForPartner.DL.Models;
using eFMS.API.ForPartner.Infrastructure.Filters;
using eFMS.API.ForPartner.Infrastructure.Middlewares;
using eFMS.API.ForPartner.Service.Models;
using ITL.NetCore.Connection.EF;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;

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
        public AccountingController(IAccountingManagementService service, 
            IStringLocalizer<LanguageSub> localizer)
        {
            accountingManagementService = service;
            stringLocalizer = localizer;
        }

        /// <summary>
        /// Test
        /// </summary>
        /// <remarks>
        /// Remark
        /// </remarks>
        /// <returns></returns>
        /// <response></response>
        [HttpPost("GenerateHash")]
        public IActionResult Test(VoucherAdvance model,[Required] string apiKey)
        {
            return Ok(accountingManagementService.GenerateHashStringTest(model,apiKey));
        }


        [HttpGet("GetInvoice")]
        public IActionResult GetInvoice()
        {
            string apiKey = Request.Headers[ForPartnerConstants.API_KEY_HEADER];
            if (!accountingManagementService.ValidateApiKey(apiKey)){
                return Unauthorized();
            }
            AccAccountingManagementModel data = accountingManagementService.GetById(Guid.NewGuid());
            return Ok(data);
        }

        [HttpPut("UpdateVoucherAdvance")]
        public IActionResult UpdateVoucherAdvance(VoucherAdvance model, [Required] string apiKey, [Required] string hash)
        {
            if (!accountingManagementService.ValidateApiKey(apiKey))
            {
                return Unauthorized();
            }
            if(!accountingManagementService.ValidateHashString(model, apiKey,hash))
            {
                return Unauthorized();

            }
            return Ok(new ResultHandle { Status = true, Message = "Update Phiếu chi thành công", Data = model });
        }


        [HttpPut("RemoveVoucherAdvance")]
        public IActionResult RemoveVoucherAdvance(string voucherNo)
        {
            return Ok(new ResultHandle { Status = true, Message = "Hủy Phiếu chi thành công", Data = voucherNo });
        }

        /// <summary>
        /// Create Invoice
        /// </summary>
        /// <param name="model">model to create invoice</param>
        /// <param name="apiKey">API Key</param>
        /// <returns></returns>
        [HttpPost("CreateInvoiceData")]
        public IActionResult CreateInvoiceData(InvoiceCreateInfo model, [Required] string apiKey, [Required] string hash)
        {
            //if (!accountingManagementService.ValidateApiKey(apiKey))
            //{
            //    return Unauthorized();
            //}
            //if (!accountingManagementService.ValidateHashString(model, apiKey, hash))
            //{
            //    return Unauthorized();
            //}

            if (!ModelState.IsValid) return BadRequest();

            var debitCharges = model.Charges.Where(x => x.ChargeType != ForPartnerConstants.TYPE_CHARGE_OBH).ToList();
            if (debitCharges.Count == 0)
            {
                ResultHandle _result = new ResultHandle { Status = false, Message = "Invoice don't have any charge in this period, Please check it again!" };
                return BadRequest(_result);
            }

            var hs = accountingManagementService.CreateInvoice(model, apiKey);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = model };
            if (!hs.Success)
            {
                result.Data = null;
                return BadRequest(result);
            }
            return Ok(result);          
        }

        /// <summary>
        /// Replace Invoice (Delete and Create Invoice)
        /// </summary>
        /// <param name="model">model to replace invoice</param>
        /// <returns></returns>
        [HttpPut("ReplaceInvoiceData")]
        public IActionResult ReplaceInvoiceData(InvoiceUpdateInfo model, [Required] string apiKey)
        {
            return Ok(new ResultHandle { Status = true, Message = "Thay thế Hóa đơn thành công", Data = model });
        }

        /// <summary>
        /// Canceling Invoice (Delete Invoice)
        /// </summary>
        /// <param name="model"></param>
        /// <param name="apiKey"></param>
        /// <returns></returns>
        [HttpPut("CancellingInvoice")]
        public IActionResult CancellingInvoice(InvoiceInfo model, [Required] string apiKey)
        {
            var hs = accountingManagementService.DeleteInvoice(model, apiKey);            
            var message = HandleError.GetMessage(hs, Crud.Delete);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = stringLocalizer[message].Value, Data = model };
            if (!hs.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [HttpPut("RejectData")]
        public IActionResult RejectData(RejectData model)
        {
            return Ok(new ResultHandle { Status = true, Message = "Reject data thành công", Data = model });

        }
    }
}