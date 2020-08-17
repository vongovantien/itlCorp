using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.Models.ReportResults.Sales;

namespace eFMS.API.Documentation.DL.IService
{
    public interface ISaleReportService
    {
        Crystal PreviewGetDepartSaleReport(SaleReportCriteria criteria);
        Crystal PreviewGetMonthlySaleReport(SaleReportCriteria criteria);
        Crystal PreviewGetQuaterSaleReport(SaleReportCriteria criteria);
        Crystal PreviewSummarySaleReport(SaleReportCriteria criteria);
        Crystal PreviewCombinationSaleReport(SaleReportCriteria criteria);


    }
}
