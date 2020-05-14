using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Exports;
using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;

namespace eFMS.API.Documentation.DL.IService
{
    public interface ICsAirWayBillService : IRepositoryBase<CsAirWayBill, CsAirWayBillModel>
    {
        CsAirWayBillModel GetBy(Guid jobId);
        HandleState Update(CsAirWayBillModel model);
        AirwayBillExportResult AirwayBillExport(Guid jobId);
        Crystal PreviewAirwayBill(Guid jobId);

    }
}
