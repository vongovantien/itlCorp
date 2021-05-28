using eFMS.API.ForPartner.DL.Models;
using eFMS.API.ForPartner.DL.Models.Receivable;
using eFMS.API.ForPartner.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;

namespace eFMS.API.ForPartner.DL.IService
{
    public interface IAccountingManagementService : IRepositoryBase<AccAccountingManagement, AccAccountingManagementModel>, IForPartnerApiService
    {
        AccAccountingManagementModel GetById(Guid id);
        string GenerateHashStringTest(object body, string apiKey);
        HandleState UpdateVoucherAdvance(VoucherAdvance model, string apiKey);
        HandleState RemoveVoucherAdvance(string voucherNo, string apiKey);
        HandleState InsertInvoice(InvoiceCreateInfo model, string apiKey);
        HandleState UpdateInvoice(InvoiceUpdateInfo model, string apiKey);
        HandleState DeleteInvoice(InvoiceInfo model, string apiKey);
        HandleState RejectData(RejectData model, string apiKey);
        HandleState RemoveVoucher(RejectData model, string apiKey);
        CalculatorReceivableNotAuthorizeModel GetCalculatorReceivableNotAuthorizeModelBySurchargeIds(List<Guid> surchargeIds, string apiKey, string action);
        List<Guid> GetSurchargeIdsByRefNoInvoice(string referenceNo);
        HandleState UpdateVoucherExpense(VoucherExpense voucherExpense, string apiKey);
    }
}
