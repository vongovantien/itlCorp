using eFMS.API.Report.DL.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Report.DL.IService
{
    public interface IGeneralReportService
    {
        List<GeneralReportResult> GetDataGeneralReport(GeneralReportCriteria criteria, int page, int size, out int rowCount);
        IQueryable<GeneralReportResult> QueryDataGeneralReport(GeneralReportCriteria criteria);
        IQueryable<GeneralExportShipmentOverviewResult> GetDataGeneralExportShipmentOverview(GeneralReportCriteria criteria);
    }
}
