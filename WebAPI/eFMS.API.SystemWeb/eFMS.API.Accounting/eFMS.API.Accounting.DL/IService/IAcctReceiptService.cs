using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.Service.Models;
using ITL.NetCore.Connection.BL;
using System;
using eFMS.API.Provider.Services.IService;
using ITL.NetCore.Common;
using System.Linq;
using eFMS.API.Accounting.DL.Models.Criteria;
using System.Collections.Generic;
using eFMS.API.Accounting.DL.Models.Receipt;
using System.Threading.Tasks;

namespace eFMS.API.Accounting.DL.IService
{
    public interface IAcctReceiptService: IRepositoryBase<AcctReceipt, AcctReceiptModel>, IPermissionBaseService<AcctReceiptModel, AcctReceipt>
    {
        IQueryable<AcctReceiptModel> Paging(AcctReceiptCriteria criteria, int page, int size, out int rowsCount);
        IQueryable<AcctReceipt> Query(AcctReceiptCriteria criteria);
        HandleState Delete(Guid id);
        string GenerateReceiptNo();
        List<ReceiptInvoiceModel> GetInvoiceForReceipt(ReceiptInvoiceCriteria criteria);
        AcctReceiptModel GetById(Guid id);
        HandleState SaveReceipt(AcctReceiptModel receiptModel, SaveAction saveAction);
        HandleState SaveDoneReceipt(Guid receiptId);
        HandleState SaveCancel(Guid receiptId);
        ProcessClearInvoiceModel ProcessReceiptInvoice(ProcessReceiptInvoice criteria);
        List<CustomerDebitCreditModel> GetDataIssueCustomerPayment(CustomerDebitCreditCriteria criteria);
        AgencyDebitCreditDetailModel GetDataIssueAgencyPayment(CustomerDebitCreditCriteria criteria);
        Task<HandleState> CalculatorReceivableForReceipt(Guid receiptId);
        bool CheckPaymentPaid(List<ReceiptInvoiceModel> Payments);
        void AlertReceiptToDeppartment(List<int> Ids, AcctReceiptModel receiptModel);
    }
}
