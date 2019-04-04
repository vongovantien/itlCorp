using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.Service.Models;
using eFMS.Domain.Report;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Documentation.DL.IService
{
    public interface ICsTransactionDetailService : IRepositoryBase<CsTransactionDetail, CsTransactionDetailModel>
    {
        IQueryable<CsTransactionDetailModel> QueryDetail(CsTransactionDetailCriteria criteria);

        List<CsTransactionDetailModel> GetByJob(CsTransactionDetailCriteria criteria);
        HandleState AddTransactionDetail(CsTransactionDetailModel model);
        HandleState UpdateTransactionDetail(CsTransactionDetailModel model);
        CsTransactionDetailReport GetReportBy(Guid jobId);
        List<CsTransactionDetailModel> Paging(CsTransactionDetailCriteria criteria, int page, int size, out int rowsCount);
        object ImportCSTransactionDetail(CsTransactionDetailModel model);
    }
}
