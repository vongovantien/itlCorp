﻿using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.Criteria;
using eFMS.API.Accounting.DL.Models.ExportResults;
using eFMS.API.Accounting.Service.Models;
using eFMS.API.Common;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Accounting.DL.IService
{
    public interface IAccountingManagementService : IRepositoryBase<AccAccountingManagement, AccAccountingManagementModel>
    {
        HandleState Delete(Guid id);
        List<PartnerOfAcctManagementResult> GetChargeSellForInvoiceByCriteria(PartnerOfAcctManagementCriteria criteria);
        List<PartnerOfAcctManagementResult> GetChargeForVoucherByCriteria(PartnerOfAcctManagementCriteria criteria);
        int CheckDeletePermission(Guid id);
        List<AccAccountingManagementResult> Paging(AccAccountingManagementCriteria criteria, int page, int size, out int rowsCount);
        HandleState AddAcctMgnt(AccAccountingManagementModel model);
        HandleState UpdateAcctMngt(AccAccountingManagementModel model);
        string GenerateVoucherId(string acctMngtType, string voucherType);
        AccAccountingManagementModel GetById(Guid id);
        AccAccountingManagementModel GetAcctMngtById(Guid id);
        string GenerateInvoiceNoTemp();
        List<AccountingManagementExport> GetDataAcctMngtExport(AccAccountingManagementCriteria criteria);
        List<AcctMngtVatInvoiceImportModel> CheckVatInvoiceImport(List<AcctMngtVatInvoiceImportModel> list);
        ResultHandle ImportVatInvoice(List<AcctMngtVatInvoiceImportModel> list);
        int CheckDetailPermission(Guid id);
        CatContractInvoiceModel GetContractForInvoice(AccMngtContractInvoiceCriteria model);
        List<Guid> GetSurchargeIdByAcctMngtId(Guid? acctMngt);
        ChargeAccountingMngtTotalViewModel CalculateListChargeAccountingMngt(List<ChargeOfAccountingManagementModel> charges);
    }
}
