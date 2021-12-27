using System.Collections.Generic;
using System.Linq;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using ITL.NetCore.Common;
using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Connection.BL;
using System;

namespace eFMS.API.Documentation.DL.IService
{
    public interface IShipmentService : IRepositoryBase<CsTransaction, CsTransactionModel>
    {
        IQueryable<Shipments> GetShipmentNotLocked();

        IQueryable<Shipments> GetShipmentsCreditPayer(string partner, List<string> services);

        List<ShipmentsCopy> GetListShipmentBySearchOptions(string searchOption, List<string> keywords);
        LockedLogResultModel GetShipmentToUnLock(ShipmentCriteria criteria);
        HandleState UnLockShipment(List<LockedLogModel> shipments);
        IQueryable<Shipments> GetShipmentNotDelete();
        List<GeneralReportResult> GetDataGeneralReport(GeneralReportCriteria criteria,int page,int size, out int rowCount);
        List<GeneralReportResult> QueryDataGeneralReport(GeneralReportCriteria criteria);
        IQueryable<GeneralExportShipmentOverviewResult> GetDataGeneralExportShipmentOverview(GeneralReportCriteria criteria);
        IQueryable<GeneralExportShipmentOverviewFCLResult> GetDataGeneralExportShipmentOverviewFCL(GeneralReportCriteria criteria);
        IQueryable<GeneralExportShipmentOverviewFCLResult> GetDataGeneralExportShipmentOverviewLCL(GeneralReportCriteria criteria);
        IQueryable<AccountingPlSheetExportResult> GetDataAccountingPLSheet(GeneralReportCriteria criteria);
        List<JobProfitAnalysisExportResult> GetDataJobProfitAnalysis(GeneralReportCriteria criteria);
        List<SummaryOfCostsIncurredExportResult> GetDataSummaryOfCostsIncurred(GeneralReportCriteria criteria);
        SummaryOfRevenueModel GetDataSummaryOfRevenueIncurred(GeneralReportCriteria criteria);
        IQueryable<Shipments> GetShipmentAssignPIC();
        IQueryable<Shipments> GetShipmentAssignPICCarrier(string type);
        CommissionExportResult GetCommissionReport(CommissionReportCriteria criteria, string userId, string rptType);
        CommissionExportResult GetIncentiveReport(CommissionReportCriteria criteria, string userId);
        SummaryOfRevenueModel GetDataCostsByPartner(GeneralReportCriteria criteria);
        HandleState LockShipmentList(List<string> JobIds);
        List<ShipmentAdvanceSettlementModel> GetAdvanceSettlements(Guid Id);
    }
}
