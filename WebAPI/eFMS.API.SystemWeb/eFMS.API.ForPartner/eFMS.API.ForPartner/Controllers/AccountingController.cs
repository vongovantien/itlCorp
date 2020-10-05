using System;
using System.Collections.Generic;
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

        [HttpPut("UpdateVoucherAdvance")]
        public IActionResult UpdateVoucherAdvance(VoucherAdvance model)
        {
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
        /// <returns></returns>
        [HttpPost("CreateInvoiceData")]
        public IActionResult CreateInvoiceData(InvoiceCreateInfo model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var hs = accountingManagementService.CreateInvoice(model);
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
        /// Replace Invoice (Update Invoice)
        /// </summary>
        /// <param name="model">model to replace invoice</param>
        /// <returns></returns>
        [HttpPut("ReplaceInvoiceData")]
        public IActionResult ReplaceInvoiceData(InvoiceUpdateInfo model)
        {
            return Ok(new ResultHandle { Status = true, Message = "Thay thế Hóa đơn thành công", Data = model });
        }

        [HttpPut("CancellingInvoice")]
        public IActionResult CancellingInvoice(InvoiceInfo model)
        {
            return Ok(new ResultHandle { Status = true, Message = "Hủy Hóa đơn thành công", Data = model });
        }

        [HttpPut("RejectData")]
        public IActionResult RejectData(RejectData model)
        {
            return Ok(new ResultHandle { Status = true, Message = "Reject data thành công", Data = model });

        }
    }
}