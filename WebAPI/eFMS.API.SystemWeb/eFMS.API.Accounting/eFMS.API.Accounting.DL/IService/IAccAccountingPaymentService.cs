﻿using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.AccountingPayment;
using eFMS.API.Accounting.DL.Models.Criteria;
using eFMS.API.Accounting.DL.Models.ExportResults;
using eFMS.API.Accounting.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Accounting.DL.IService
{
    public interface IAccAccountingPaymentService : IRepositoryBase<AccAccountingPayment, AccAccountingPaymentModel>
    {
        IQueryable<AccAccountingPaymentModel> GetBy(string refId, string refNo);
        IQueryable<AccountingPaymentModel> Paging(PaymentCriteria criteria, int page, int size, out int rowsCount);
        HandleState UpdateExtendDate(ExtendDateUpdatedModel model);
        HandleState Delete(Guid id);
        List<AccountingPaymentImportModel> CheckValidImportInvoicePayment(List<AccountingPaymentImportModel> list);
        HandleState ImportInvoicePayment(List<AccountingPaymentImportModel> list);
        ExtendDateUpdatedModel GetInvoiceExtendedDate(string refNo);
        ExtendDateUpdatedModel GetOBHSOAExtendedDate(string id);
        HandleState ImportOBHPayment(List<AccountingPaymentOBHImportTemplateModel> list);
        List<AccountingPaymentOBHImportTemplateModel> CheckValidImportOBHPayment(List<AccountingPaymentOBHImportTemplateModel> dataList);
        IQueryable<AccountingPaymentModel> ExportAccountingPayment(PaymentCriteria criteria);
        IQueryable<AccountingCustomerPaymentExport> GetDataExportAccountingCustomerPayment(PaymentCriteria criteria);
        IQueryable<AccountingAgencyPaymentExport> GetDataExportAccountingAgencyPayment(PaymentCriteria criteria);
    }
}
