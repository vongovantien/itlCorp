using eFMS.API.Report.DL.Models;
using System.Linq;

namespace eFMS.API.Report.DL.IService
{
    public interface IAccountingReportService
    {
        IQueryable<AccountingPlSheetExportResult> GetDataAccountingPLSheet(GeneralReportCriteria criteria);
        IQueryable<JobProfitAnalysisExportResult> GetDataJobProfitAnalysis(GeneralReportCriteria criteria);
        IQueryable<SummaryOfCostsIncurredExportResult> GetDataSummaryOfCostsIncurred(GeneralReportCriteria criteria);
        SummaryOfRevenueModel GetDataSummaryOfRevenueIncurred(GeneralReportCriteria criteria);
        SummaryOfRevenueModel GetDataCostsByPartner(GeneralReportCriteria criteria);
    }
}
