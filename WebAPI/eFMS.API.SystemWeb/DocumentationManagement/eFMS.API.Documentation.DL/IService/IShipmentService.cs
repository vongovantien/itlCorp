using eFMS.API.Documentation.DL.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using ITL.NetCore.Common;
using eFMS.API.Common;
using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Connection.BL;
using eFMS.API.Documentation.DL.Models.ReportResults.Sales;

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
        IQueryable<GeneralExportShipmentOverviewResult> GetDataGeneralExportShipmentOverview(GeneralReportCriteria criteria);
    }
}
