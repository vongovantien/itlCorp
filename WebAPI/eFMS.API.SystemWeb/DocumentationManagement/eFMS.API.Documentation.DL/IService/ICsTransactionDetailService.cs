using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.Service.Models;
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
        HandleState DeleteTransactionDetail(Guid hbId);
        //CsTransactionDetailReport GetReportBy(Guid jobId);
        List<CsTransactionDetailModel> Query(CsTransactionDetailCriteria criteria);
        List<CsTransactionDetailModel> Paging(CsTransactionDetailCriteria criteria, int page, int size, out int rowsCount);
        object GetGoodSummaryOfAllHBLByJobId(Guid jobId);
        object ImportCSTransactionDetail(CsTransactionDetailModel model);
        //CsTransactionDetailModel GetHbDetails(Guid JobId, Guid HbId);
        Crystal Preview(CsTransactionDetailModel model);

        //CsTransactionDetailModel GetById(CsTransactionDetailCriteria csTransactionDetailCriteria);
        CsTransactionDetailModel GetById(Guid Id);
        Crystal PreviewProofOfDelivery(Guid Id);

        Crystal PreviewSeaHBLofLading(Guid hblId, string reportType);

        Crystal PreviewHouseAirwayBillLastest(Guid hblId, string reportType);
    }
}
