using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.Models.ReportResults.Sales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Documentation.DL.IService
{
    public interface ISaleReportService
    {
        //IQueryable<MonthlySaleReportResult> GetMonthlySaleReport(SaleReportCriteria criteria);
        Crystal PreviewGetMonthlySaleReport(SaleReportCriteria criteria);
    }
}
