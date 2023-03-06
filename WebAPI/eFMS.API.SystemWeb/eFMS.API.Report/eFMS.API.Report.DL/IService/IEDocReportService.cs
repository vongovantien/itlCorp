using eFMS.API.Report.DL.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Report.DL.IService
{
    public interface IEDocReportService
    {
        List<EDocReportResult> QueryDataEDocsReport(GeneralReportCriteria criteria);
    }
}
