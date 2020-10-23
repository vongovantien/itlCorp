using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Infrastructure.Common;
using eFMS.API.ForPartner.DL.Common;
using eFMS.API.ForPartner.DL.IService;
using eFMS.API.ForPartner.DL.Models;
using eFMS.API.ForPartner.Infrastructure.Middlewares;
using ITL.NetCore.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using eFMS.API.ForPartner.Infrastructure.Extensions;
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
        public IActionResult Test(object model, [Required] [DefaultValue("b2dc38f39f6f202141f46afe66276075")]string apiKey)
        {
            return Ok(accountingManagementService.GenerateHashStringTest(model, apiKey));
        }

        [HttpPost("CheckHash")]
        public IActionResult CheckHash(object model, [Required] string apiKey,[Required] string hash)
        {
            return Ok(accountingManagementService.ValidateHashString(model, apiKey,hash));
        }


        [HttpPut("UpdateVoucherAdvance")]
        public IActionResult UpdateVoucherAdvance(VoucherAdvance model, [Required] string apiKey, [Required] string hash)
        {
            if (!accountingManagementService.ValidateApiKey(apiKey))
            {
                return new CustomUnauthorizedResult("API Key invalid");
            }
            if (!accountingManagementService.ValidateHashString(model, apiKey, hash))
            {
                return new CustomUnauthorizedResult("Hashed string invalid");
            }

            HandleState hs = accountingManagementService.UpdateVoucherAdvance(model, apiKey);
            if (!hs.Success)
            {
                return BadRequest(new ResultHandle { Status = false, Message = hs.Exception.Message.ToString(), Data = model });
            }

            string message = HandleError.GetMessage(hs, Crud.Update);

            return Ok(new ResultHandle { Status = true, Message = stringLocalizer[message].Value, Data = model });
        }


        [HttpPut("RemoveVoucherAdvance")]
        public IActionResult RemoveVoucherAdvance(string voucherNo, [Required] string apiKey, [Required] string hash)
        {
            if (!accountingManagementService.ValidateApiKey(apiKey))
            {
                return new CustomUnauthorizedResult("API Key invalid");
            }
            if (!accountingManagementService.ValidateHashString(voucherNo, apiKey, hash))
            {
                return new CustomUnauthorizedResult("Hashed string invalid");
            }

            HandleState hs = accountingManagementService.RemoveVoucherAdvance(voucherNo, apiKey);
            string message = HandleError.GetMessage(hs, Crud.Update);

            if (!hs.Success)
            {
                return BadRequest(new ResultHandle { Status = false, Message = stringLocalizer[message].Value, Data = voucherNo });
            }

            return Ok(new ResultHandle { Status = true, Message = stringLocalizer[message].Value, Data = voucherNo });
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
                return new CustomUnauthorizedResult("API Key invalid");
            }
            if (!accountingManagementService.ValidateHashString(model, apiKey, hash))
            {
                return new CustomUnauthorizedResult("Hashed string invalid");
            }

            if (!ModelState.IsValid) return BadRequest();

            var debitCharges = model.Charges.Where(x => x.ChargeType?.ToUpper() == ForPartnerConstants.TYPE_DEBIT).ToList();
            if (debitCharges.Count == 0)
            {
                ResultHandle _result = new ResultHandle { Status = false, Message = "Không có phí để tạo hóa đơn. Vui lòng kiểm tra lại!", Data = model };
                return BadRequest(_result);
            }

            var hs = accountingManagementService.InsertInvoice(model, apiKey);
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = "Tạo mới hóa đơn thành công", Data = model };
            if (!hs.Success)
            {
                ResultHandle _result = new ResultHandle { Status = hs.Success, Message = hs.Message.ToString() + ". Tạo mới hóa đơn thất bại", Data = model };
                return BadRequest(_result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Replace Invoice (Delete old and Create New Invoice)
        /// </summary>
        /// <param name="model">model to replace invoice</param>
        /// <param name="apiKey">API Key</param>
        /// <param name="hash"></param>
        /// <returns></returns>
        [HttpPut("ReplaceInvoiceData")]
        public IActionResult ReplaceInvoiceData(InvoiceUpdateInfo model, [Required] string apiKey, [Required] string hash)
        {
            if (!accountingManagementService.ValidateApiKey(apiKey))
            {
                return new CustomUnauthorizedResult("API Key invalid");
            }
            if (!accountingManagementService.ValidateHashString(model, apiKey, hash))
            {
                return new CustomUnauthorizedResult("Hashed string invalid");
            }

            if (!ModelState.IsValid) return BadRequest();

            var debitCharges = model.Charges.Where(x => x.ChargeType?.ToUpper() == ForPartnerConstants.TYPE_DEBIT).ToList();
            if (debitCharges.Count == 0)
            {
                ResultHandle _result = new ResultHandle { Status = false, Message = "Không có phí để thay thế hóa đơn. Vui lòng kiểm tra lại!", Data = model };
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
                ResultHandle _result = new ResultHandle { Status = hsDeleteInvoice.Success, Message = hsDeleteInvoice.Message.ToString() + ". Xóa hóa đơn cũ thất bại", Data = model };
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
                Charges = model.Charges,
                PaymentTerm = model.PaymentTerm
            };            
            invoiceToCreate.Charges.ForEach(fe => {
                fe.ReferenceNo = model.ReferenceNo;
            });
            var hsInsertInvoice = accountingManagementService.InsertInvoice(invoiceToCreate, apiKey);
            #endregion --- Create New Invoice by ReferenceNo ---

            ResultHandle result = new ResultHandle { Status = hsInsertInvoice.Success, Message = "Thay thế hóa đơn thành công", Data = model };
            if (!hsInsertInvoice.Success)
            {
                ResultHandle _result = new ResultHandle { Status = hsInsertInvoice.Success, Message = hsInsertInvoice.Message.ToString() + ". Thay thế hóa đơn thất bại", Data = model };
                return BadRequest(_result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Canceling Invoice (Delete Invoice)
        /// </summary>
        /// <param name="model">model to canceling invoice</param>
        /// <param name="apiKey">API Key</param>
        /// <param name="hash"></param>
        /// <returns></returns>
        [HttpPut("CancellingInvoice")]
        public IActionResult CancellingInvoice(InvoiceInfo model, [Required] string apiKey, [Required] string hash)
        {
            if (!accountingManagementService.ValidateApiKey(apiKey))
            {
                return new CustomUnauthorizedResult("API Key invalid");
            }
            if (!accountingManagementService.ValidateHashString(model, apiKey, hash))
            {
                return new CustomUnauthorizedResult("Hashed string invalid");
            }
            if (!ModelState.IsValid) return BadRequest();

            var hs = accountingManagementService.DeleteInvoice(model, apiKey);            
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = "Hủy hóa đơn thành công", Data = model };
            if (!hs.Success)
            {
                ResultHandle _result = new ResultHandle { Status = hs.Success, Message = hs.Message.ToString() + ". Hủy hóa đơn thất bại", Data = model };
                return BadRequest(_result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Reject Data
        /// </summary>
        /// <param name="model">model to reject data</param>
        /// <param name="apiKey">API Key</param>
        /// <param name="hash"></param>
        /// <returns></returns>
        [HttpPut("RejectData")]
        public IActionResult RejectData(RejectData model, [Required] string apiKey, [Required] string hash)
        {
            if (!accountingManagementService.ValidateApiKey(apiKey))
            {
                return new CustomUnauthorizedResult("API Key invalid");
            }
            if (!accountingManagementService.ValidateHashString(model, apiKey, hash))
            {
                return new CustomUnauthorizedResult("Hashed string invalid");
            }
            if (!ModelState.IsValid) return BadRequest();
            var hs = accountingManagementService.RejectData(model, apiKey);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = "Reject data thành công", Data = model };
            if (!hs.Success)
            {
                ResultHandle _result = new ResultHandle { Status = hs.Success, Message = hs.Message.ToString() + ". Reject data thất bại", Data = model };
                return BadRequest(_result);
            }
            return Ok(result);
        }
    }
}