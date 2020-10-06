using eFMS.API.ForPartner.DL.Models;
using eFMS.API.ForPartner.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;

namespace eFMS.API.ForPartner.DL.IService
{
    public interface IAccountingManagementService: IRepositoryBase<AccAccountingManagement, AccAccountingManagementModel>, IForPartnerApiService
    {
        AccAccountingManagementModel GetById(Guid id);
        string GenerateHashStringTest(object body, string apiKey);
        HandleState UpdateVoucherAdvance(VoucherAdvance model);
        HandleState InsertInvoice(InvoiceCreateInfo model, string apiKey);
        HandleState UpdateInvoice(InvoiceUpdateInfo model, string apiKey);
        HandleState DeleteInvoice(InvoiceInfo model, string apiKey);
    }
}
