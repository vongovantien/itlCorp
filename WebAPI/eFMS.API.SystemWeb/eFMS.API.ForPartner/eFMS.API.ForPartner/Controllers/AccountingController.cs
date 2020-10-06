using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.API.ForPartner.DL.Common;
using eFMS.API.ForPartner.DL.IService;
using eFMS.API.ForPartner.DL.Models;
using eFMS.API.ForPartner.Infrastructure.Middlewares;
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
        /// <param name="hash"></param>
        /// <returns></returns>
        [HttpPost("CreateInvoiceData")]
        public IActionResult CreateInvoiceData(InvoiceCreateInfo model, [Required] string apiKey, [Required] string hash)
        {
            if (!accountingManagementService.ValidateApiKey(apiKey))
            {
                return Unauthorized();
            }
            //Tạm thời comment
            //if (!accountingManagementService.ValidateHashString(model, apiKey, hash))
            //{
            //    return Unauthorized();
            //}

            if (!ModelState.IsValid) return BadRequest();

            var debitCharges = model.Charges.Where(x => x.ChargeType?.ToUpper() == ForPartnerConstants.TYPE_DEBIT).ToList();
            if (debitCharges.Count == 0)
            {
                ResultHandle _result = new ResultHandle { Status = false, Message = "Không có phí để tạo hóa đơn. Vui lòng kiểm tra lại!" };
                return BadRequest(_result);
            }

            var hs = accountingManagementService.InsertInvoice(model, apiKey);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = "Tạo mới hóa đơn thành công", Data = model };
            if (!hs.Success)
            {
                ResultHandle _result = new ResultHandle { Status = hs.Success, Message = "Tạo mới hóa đơn thất bại" };
                return BadRequest(_result);
            }
            return Ok(result);          
        }

        /// <summary>
        /// Replace Invoice (Delete and Create Invoice)
        /// </summary>
        /// <param name="model">model to replace invoice</param>
        /// <param name="apiKey"></param>
        /// <param name="hash"></param>
        /// <returns></returns>
        [HttpPut("ReplaceInvoiceData")]
        public IActionResult ReplaceInvoiceData(InvoiceUpdateInfo model, [Required] string apiKey, [Required] string hash)
        {
            if (!accountingManagementService.ValidateApiKey(apiKey))
            {
                return Unauthorized();
            }
            //Tạm thời comment
            //if (!accountingManagementService.ValidateHashString(model, apiKey, hash))
            //{
            //    return Unauthorized();
            //}

            if (!ModelState.IsValid) return BadRequest();

            var debitCharges = model.Charges.Where(x => x.ChargeType?.ToUpper() == ForPartnerConstants.TYPE_DEBIT).ToList();
            if (debitCharges.Count == 0)
            {
                ResultHandle _result = new ResultHandle { Status = false, Message = "Không có phí để thay thế hóa đơn. Vui lòng kiểm tra lại!" };
                return BadRequest(_result);
            }

            #region --- Delete Invoice Old by PreReferenceNo ---
            var invoiceToDelete = new InvoiceInfo
            {
                ReferenceNo = model.PreReferenceNo
            };
            var hsDeleteInvoice = accountingManagementService.DeleteInvoice(invoiceToDelete, apiKey);
            if (!hsDeleteInvoice.Success)
            {
                ResultHandle _result = new ResultHandle { Status = hsDeleteInvoice.Success, Message = "Xóa hóa đơn cũ thất bại" };
                return BadRequest(_result);
            }
            #endregion --- Delete Invoice Old by PreReferenceNo ---

            #region --- Create New Invoice by ReferenceNo ---
            var invoiceToCreate = new InvoiceCreateInfo
            {
                PartnerCode = model.PartnerCode,
                InvoiceNo = model.InvoiceNo,
                InvoiceDate = model.InvoiceDate,
                SerieNo = model.SerieNo,
                Currency = model.Currency,
                Charges = model.Charges
            };            
            invoiceToCreate.Charges.ForEach(fe => {
                fe.ReferenceNo = model.ReferenceNo;
            });
            var hsInsertInvoice = accountingManagementService.InsertInvoice(invoiceToCreate, apiKey);
            #endregion --- Create New Invoice by ReferenceNo ---

            ResultHandle result = new ResultHandle { Status = hsInsertInvoice.Success, Message = "Thay thế hóa đơn thành công", Data = model };
            if (!hsInsertInvoice.Success)
            {
                ResultHandle _result = new ResultHandle { Status = hsInsertInvoice.Success, Message = "Thay thế hóa đơn thất bại", Data = model };
                return BadRequest(_result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Canceling Invoice (Delete Invoice)
        /// </summary>
        /// <param name="model"></param>
        /// <param name="apiKey"></param>
        /// <param name="hash"></param>
        /// <returns></returns>
        [HttpPut("CancellingInvoice")]
        public IActionResult CancellingInvoice(InvoiceInfo model, [Required] string apiKey, [Required] string hash)
        {
            if (!accountingManagementService.ValidateApiKey(apiKey))
            {
                return Unauthorized();
            }
            //Tạm thời comment
            //if (!accountingManagementService.ValidateHashString(model, apiKey, hash))
            //{
            //    return Unauthorized();
            //}
            if (!ModelState.IsValid) return BadRequest();

            var hs = accountingManagementService.DeleteInvoice(model, apiKey);            
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = "Hủy hóa đơn thành công", Data = model };
            if (!hs.Success)
            {
                ResultHandle _result = new ResultHandle { Status = hs.Success, Message = "Hủy hóa đơn thất bại" };
                return BadRequest(_result);
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