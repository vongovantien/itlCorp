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
using System;
using System.Collections.Generic;

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
        public IActionResult CheckHash(object model, [Required] string apiKey, [Required] string hash)
        {
            return Ok(accountingManagementService.ValidateHashString(model, apiKey, hash));
        }

        /// <summary>
        /// Update Voucher Advance
        /// </summary>
        /// <param name="model"></param>
        /// <param name="apiKey"></param>
        /// <param name="hash"></param>
        /// <returns></returns>
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

            var fieldRequireVoucherAdvance = GetFieldRequireForUpdateVoucherAdvance(model);
            if (!string.IsNullOrEmpty(fieldRequireVoucherAdvance))
            {
                ResultHandle _result = new ResultHandle { Status = false, Message = string.Format(@"Trường {0} không có dữ liệu. Vui lòng kiểm tra lại!", fieldRequireVoucherAdvance), Data = model };
                return BadRequest(_result);
            }

            HandleState hs = accountingManagementService.UpdateVoucherAdvance(model, apiKey);
            if (!hs.Success)
            {
                return BadRequest(new ResultHandle { Status = false, Message = hs.Exception.Message.ToString(), Data = model });
            }

            string message = HandleError.GetMessage(hs, Crud.Update);

            return Ok(new ResultHandle { Status = true, Message = stringLocalizer[message].Value, Data = model });
        }

        /// <summary>
        /// Remove Voucher Advance
        /// </summary>
        /// <param name="voucherNo"></param>
        /// <param name="apiKey"></param>
        /// <param name="hash"></param>
        /// <returns></returns>
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

            var fieldRequireInvoice = GetFieldRequireForCreateInvoice(model);
            if (!string.IsNullOrEmpty(fieldRequireInvoice))
            {
                ResultHandle _result = new ResultHandle { Status = false, Message = string.Format(@"Trường {0} không có dữ liệu. Vui lòng kiểm tra lại!", fieldRequireInvoice), Data = model };
                return BadRequest(_result);
            }

            var fieldRequireCharge = GetFieldRequireCharges(model.Charges);
            if (!string.IsNullOrEmpty(fieldRequireCharge))
            {
                ResultHandle _result = new ResultHandle { Status = false, Message = string.Format(@"Các trường của ds charge: [{0}] không có dữ liệu. Vui lòng kiểm tra lại!", fieldRequireCharge), Data = model };
                return BadRequest(_result);
            }
            
            var debit_Obh_Charges = model.Charges.Where(x => x.ChargeType?.ToUpper() == ForPartnerConstants.TYPE_DEBIT || x.ChargeType?.ToUpper() == ForPartnerConstants.TYPE_CHARGE_OBH).ToList();
            if (debit_Obh_Charges.Count == 0)
            {
                ResultHandle _result = new ResultHandle { Status = false, Message = "Không có phí để tạo hóa đơn. Vui lòng kiểm tra lại!", Data = model };
                return BadRequest(_result);
            }
            
            var hs = accountingManagementService.InsertInvoice(model, apiKey, "CreateInvoiceData");
            var message = HandleError.GetMessage(hs, Crud.Insert);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = "Tạo mới hóa đơn thành công", Data = model };
            if (!hs.Success)
            {
                ResultHandle _result = new ResultHandle { Status = hs.Success, Message = string.Format(@"{0}. Tạo mới hóa đơn thất bại", hs.Message.ToString()), Data = model };
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

            var fieldRequireInvoice = GetFieldRequireForUpdateInvoice(model);
            if (!string.IsNullOrEmpty(fieldRequireInvoice))
            {
                ResultHandle _result = new ResultHandle { Status = false, Message = string.Format(@"Trường {0} không có dữ liệu. Vui lòng kiểm tra lại!", fieldRequireInvoice), Data = model };
                return BadRequest(_result);
            }

            var fieldRequireCharge = GetFieldRequireCharges(model.Charges);
            if (!string.IsNullOrEmpty(fieldRequireCharge))
            {
                ResultHandle _result = new ResultHandle { Status = false, Message = string.Format(@"Các trường của ds phí: [{0}] không có dữ liệu. Vui lòng kiểm tra lại!", fieldRequireCharge), Data = model };
                return BadRequest(_result);
            }

            var debit_Obh_Charges = model.Charges.Where(x => x.ChargeType?.ToUpper() == ForPartnerConstants.TYPE_DEBIT || x.ChargeType?.ToUpper() == ForPartnerConstants.TYPE_CHARGE_OBH).ToList();
            if (debit_Obh_Charges.Count == 0)
            {
                ResultHandle _result = new ResultHandle { Status = false, Message = "Không có phí để thay thế hóa đơn. Vui lòng kiểm tra lại!", Data = model };
                return BadRequest(_result);
            }
            
            #region --- Delete Invoice Old by PreReferenceNo ---
            var invoiceToDelete = new InvoiceInfo
            {
                ReferenceNo = model.PreReferenceNo
            };
            var hsDeleteInvoice = accountingManagementService.DeleteInvoice(invoiceToDelete, apiKey, "ReplaceInvoiceData");
            if (!hsDeleteInvoice.Success)
            {
                ResultHandle _result = new ResultHandle { Status = hsDeleteInvoice.Success, Message = string.Format(@"{0}. Xóa hóa đơn cũ thất bại", hsDeleteInvoice.Message.ToString()), Data = model };
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
                Description = model.Description
            };
            invoiceToCreate.Charges.ForEach(fe =>
            {
                fe.ReferenceNo = model.ReferenceNo;
            });
            var hsInsertInvoice = accountingManagementService.InsertInvoice(invoiceToCreate, apiKey, "ReplaceInvoiceData");
            #endregion --- Create New Invoice by ReferenceNo ---

            ResultHandle result = new ResultHandle { Status = hsInsertInvoice.Success, Message = "Thay thế hóa đơn thành công", Data = model };
            if (!hsInsertInvoice.Success)
            {
                ResultHandle _result = new ResultHandle { Status = hsInsertInvoice.Success, Message = string.Format(@"{0}. Thay thế hóa đơn thất bại", hsInsertInvoice.Message.ToString()), Data = model };
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

            var hs = accountingManagementService.DeleteInvoice(model, apiKey, "CancellingInvoice");
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = "Hủy hóa đơn thành công", Data = model };
            if (!hs.Success)
            {
                ResultHandle _result = new ResultHandle { Status = hs.Success, Message = string.Format(@"{0}. Hủy hóa đơn thất bại", hs.Message.ToString()), Data = model };
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
            ResultHandle result = new ResultHandle { Status = true, Message = "Reject data thành công", Data = model };
            if (!hs.Success)
            {
                ResultHandle _result = new ResultHandle { Status = hs.Success, Message = string.Format(@"{0}. Reject data thất bại", hs.Message.ToString()), Data = model };
                return BadRequest(_result);
            }
            return Ok(result);
        }

        /// <summary>
        /// Remove Voucher
        /// </summary>
        /// <param name="model">model to remove data</param>
        /// <param name="apiKey">API Key</param>
        /// <param name="hash"></param>
        /// <returns></returns>
        [HttpPut("RemoveVoucher")]
        public IActionResult RemoveVoucher(RejectData model, [Required] string apiKey, [Required] string hash)
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
            var hs = accountingManagementService.RemoveVoucher(model, apiKey);
            ResultHandle result = new ResultHandle { Status = hs.Success, Message = "Remove voucher thành công", Data = model };
            if (!hs.Success)
            {
                ResultHandle _result = new ResultHandle { Status = hs.Success, Message = string.Format(@"{0}. Remove voucher thất bại", hs.Message.ToString()), Data = model };
                return BadRequest(_result);
            }
            return Ok(result);
        }

        #region --- PRIVATE ---
        private string GetFieldRequireForCreateInvoice(InvoiceCreateInfo model)
        {
            string message = string.Empty;
            const string comma = ", ";
            if (string.IsNullOrEmpty(model.PartnerCode))
            {
                message += "PartnerCode";
            }
            if (string.IsNullOrEmpty(model.InvoiceNo))
            {
                message += (!string.IsNullOrEmpty(message) ? comma : string.Empty) + "InvoiceNo";
            }
            if (string.IsNullOrEmpty(model.SerieNo))
            {
                message += (!string.IsNullOrEmpty(message) ? comma : string.Empty) + "SerieNo";
            }
            if (model.InvoiceDate == null)
            {
                message += (!string.IsNullOrEmpty(message) ? comma : string.Empty) + "InvoiceDate";
            }
            if (string.IsNullOrEmpty(model.Currency))
            {
                message += (!string.IsNullOrEmpty(message) ? comma : string.Empty) + "Currency";
            }
            
            return message;
        }

        private string GetFieldRequireCharges(List<ChargeInvoice> charges)
        {
            string messageCharge = string.Empty;
            const string comma = ", ";
            if (charges.Count > 0)
            {
                for (var i = 0; i < charges.Count; i++)
                {
                    string message = string.Empty;
                    if (charges[i].ChargeId == null || charges[i].ChargeId == Guid.Empty)
                    {
                        message = "ChargeId";
                    }
                    if (string.IsNullOrEmpty(charges[i].Currency))
                    {
                        message += (!string.IsNullOrEmpty(message) ? comma : string.Empty) + "Currency";
                    }
                    if (string.IsNullOrEmpty(charges[i].ReferenceNo))
                    {
                        message += (!string.IsNullOrEmpty(message) ? comma : string.Empty) + "ReferenceNo";
                    }
                    if (charges[i].PaymentTerm == null || charges[i].PaymentTerm < 1)
                    {
                        message += (!string.IsNullOrEmpty(message) ? comma : string.Empty) + "PaymentTerm (bắt buộc > 0)";
                    }
                    if (!string.IsNullOrEmpty(message))
                    {
                        messageCharge += (!string.IsNullOrEmpty(messageCharge) ? comma : string.Empty) + "{ Charge " + (i + 1) + ": " + message + " }";
                    }
                }
            }
            return messageCharge;
        }

        private string GetFieldRequireForUpdateInvoice(InvoiceUpdateInfo model)
        {
            string message = string.Empty;
            const string comma = ", ";
            if (string.IsNullOrEmpty(model.PartnerCode))
            {
                message += "PartnerCode";
            }
            if (string.IsNullOrEmpty(model.InvoiceNo))
            {
                message += (!string.IsNullOrEmpty(message) ? comma : string.Empty) + "InvoiceNo";
            }
            if (string.IsNullOrEmpty(model.SerieNo))
            {
                message += (!string.IsNullOrEmpty(message) ? comma : string.Empty) + "SerieNo";
            }
            if (model.InvoiceDate == null)
            {
                message += (!string.IsNullOrEmpty(message) ? comma : string.Empty) + "InvoiceDate";
            }
            if (string.IsNullOrEmpty(model.Currency))
            {
                message += (!string.IsNullOrEmpty(message) ? comma : string.Empty) + "Currency";
            }
            
            return message;
        }

        private string GetFieldRequireForUpdateVoucherAdvance(VoucherAdvance model)
        {
            string message = string.Empty;
            const string comma = ", ";
            if (string.IsNullOrEmpty(model.VoucherNo))
            {
                message += "VoucherNo";
            }
            if (model.VoucherDate == null)
            {
                message += (!string.IsNullOrEmpty(message) ? comma : string.Empty) + "VoucherDate";
            }
            if (model.PaymentTerm == null || model.PaymentTerm < 1)
            {
                message += (!string.IsNullOrEmpty(message) ? comma : string.Empty) + "PaymentTerm (bắt buộc > 0)";
            }
            if (string.IsNullOrEmpty(model.AdvanceNo))
            {
                message += (!string.IsNullOrEmpty(message) ? comma : string.Empty) + "AdvanceNo";
            }
            return message;
        }
        #endregion --- PRIVATE ---

    }
}