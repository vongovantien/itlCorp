using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.Accounting;
using eFMS.API.Accounting.DL.Models.AccountingPayable;
using eFMS.API.Accounting.DL.Models.Criteria;
using eFMS.API.Accounting.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Accounting.DL.IService
{
    public interface IAcctPayableService : IRepositoryBase<AccAccountPayable, AccAccountPayableModel>
    {
        IQueryable<AccAccountPayableModel> Paging(AccountPayableCriteria criteria, int page, int size, out int rowsCount);
        List<AcctPayablePaymentExport> GetDataExportPayablePaymentDetail(AccountPayableCriteria criteria);
        List<AccountingTemplateExport> GetDataExportAccountingTemplate(AccountPayableCriteria criteria);
        IQueryable<AccAccountPayablePaymentModel> GetBy(AcctPayableViewDetailCriteria criteria);
        GeneralAccPayableModel GetGeneralPayable(string partnerId,string currency);
    }
}
