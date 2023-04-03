﻿using eFMS.API.Accounting.DL.Models;
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
using eFMS.API.Accounting.DL.Models.ExportResults;

namespace eFMS.API.Accounting.DL.IService
{
    public interface IAcctReceiptService : IRepositoryBase<AcctReceipt, AcctReceiptModel>, IPermissionBaseService<AcctReceiptModel, AcctReceipt>
    {
        IQueryable<AcctReceiptModel> Paging(AcctReceiptCriteria criteria, int page, int size, out int rowsCount);
        IQueryable<AcctReceipt> Query(AcctReceiptCriteria criteria);
        HandleState Delete(Guid id);
        string GenerateReceiptNoV2(AcctReceiptModel receipt, string officeCode);
        string GenerateReceiptNo();
        List<ReceiptInvoiceModel> GetInvoiceForReceipt(ReceiptInvoiceCriteria criteria);
        AcctReceiptModel GetById(Guid id);
        HandleState SaveReceipt(AcctReceiptModel receiptModel, SaveAction saveAction);
        HandleState SaveDoneReceipt(Guid receiptId);
        HandleState SaveCancel(Guid receiptId);
        ProcessClearInvoiceModel ProcessReceiptInvoice(ProcessReceiptInvoice criteria);
        IQueryable<CustomerDebitCreditModel> GetDataIssueCustomerPayment(CustomerDebitCreditCriteria criteria);
        AgencyDebitCreditDetailModel GetDataIssueAgencyPayment(CustomerDebitCreditCriteria criteria);
        AgencyDebitCreditDetailModel GetDataIssueCreditAgency(CustomerDebitCreditCriteria criteria);
        // Task<HandleState> CalculatorReceivableForReceipt(Guid receiptId);
        List<ObjectReceivableModel> GetListReceivableReceipt(Guid receiptId);
        bool CheckPaymentPaid(List<ReceiptInvoiceModel> Payments);
        void AlertReceiptToDeppartment(List<int> Ids, AcctReceiptModel receiptModel);
        Task<AcctReceiptAdvanceModelExport> GetDataExportReceiptAdvance(AcctReceiptCriteria criteria);
        bool ValidateCusAgreement(Guid agreementId, decimal cusVnd, decimal cusUsd);
        Task<HandleState> QuickUpdate(Guid Id, ReceiptQuickUpdateModel model);
        HandleState UpdateAccountingDebitAR(List<ReceiptInvoiceModel> payments, SaveAction saveAction);
        HandleState SaveCombineReceipt(List<AcctReceiptModel> receiptModels, SaveAction saveAction);
        List<AcctReceiptModel> GetByReceiptCombine(string _arcbNo);
        HandleState CheckExitedCombineReceipt(List<AcctReceiptModel> receiptModels);
        HandleState UpdateCreditARCombine(List<AcctReceiptModel> receiptModels, SaveAction saveAction);
        HandleState AddPaymentsCreditCombine(List<AcctReceiptModel> receiptModels, SaveAction saveAction);
    }
}
