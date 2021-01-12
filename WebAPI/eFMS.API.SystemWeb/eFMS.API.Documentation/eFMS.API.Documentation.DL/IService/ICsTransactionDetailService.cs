using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.DL.Models.Exports;
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
        string GenerateHBLNo(TransactionTypeEnum transactionTypeEnum);
        HandleState DeleteTransactionDetail(Guid hbId);
        //CsTransactionDetailReport GetReportBy(Guid jobId);
        List<CsTransactionDetailModel> Query(CsTransactionDetailCriteria criteria);
        
        IQueryable<CsTransactionDetail> GetHouseBill(string transactionType);

        IQueryable<CsTransactionDetailModel> GetListHouseBillAscHBL(CsTransactionDetailCriteria criteria);
        List<CsTransactionDetailModel> Paging(CsTransactionDetailCriteria criteria, int page, int size, out int rowsCount);
        object GetGoodSummaryOfAllHBLByJobId(Guid jobId);
        object ImportCSTransactionDetail(CsTransactionDetailModel model);
        //CsTransactionDetailModel GetHbDetails(Guid JobId, Guid HbId);
        string GenerateHBLNoSeaExport(string podCode);

        int CheckDetailPermission(Guid id);


        //CsTransactionDetailModel GetById(CsTransactionDetailCriteria csTransactionDetailCriteria);
        CsTransactionDetailModel GetById(Guid Id);
        CsTransactionDetailModel GetSeparateByHblid(Guid hbId);

        Crystal PreviewProofOfDelivery(Guid Id);
        Crystal PreviewAirProofOfDelivery(Guid Id);

        Crystal PreviewAirDocumentRelease(Guid Id);

        Crystal PreviewSeaHBLofLading(Guid hblId, string reportType);

        Crystal PreviewHouseAirwayBillLastest(Guid hblId, string reportType);

        Crystal PreviewAirAttachList(Guid hblId);
        Crystal PreviewAirImptAuthorisedLetter(Guid housbillId);
        Crystal PreviewAirImptAuthorisedLetterConsign(Guid housbillId);

        CsTransactionDetailModel GetDetails(Guid id);
        AirwayBillExportResult NeutralHawbExport(Guid housebillId, Guid officeId);
        Crystal PreviewBookingNote(BookingNoteCriteria criteria);

        IQueryable<CsTransactionDetailModel> GetDataHawbToCheckExisted();
        HandleState UpdateInputBKNote(BookingNoteCriteria criteria);
        List<HousebillDailyExportResult> GetHousebillsDailyExport(DateTime? issuedDate);
    }
}
