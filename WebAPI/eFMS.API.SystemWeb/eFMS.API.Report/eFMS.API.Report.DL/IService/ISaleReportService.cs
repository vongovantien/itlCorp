using eFMS.API.Common.Globals;
using eFMS.API.Report.DL.Models;

namespace eFMS.API.Report.DL.IService
{
    public interface ISaleReportService
    {
        Crystal PreviewGetDepartSaleReport(SaleReportCriteria criteria);
        Crystal PreviewGetMonthlySaleReport(SaleReportCriteria criteria);
        Crystal PreviewGetQuaterSaleReport(SaleReportCriteria criteria);
        Crystal PreviewSummarySaleReport(SaleReportCriteria criteria);
        Crystal PreviewCombinationSaleReport(SaleReportCriteria criteria);
        Crystal PreviewSaleKickBackReport(SaleReportCriteria criteria);

    }
}
