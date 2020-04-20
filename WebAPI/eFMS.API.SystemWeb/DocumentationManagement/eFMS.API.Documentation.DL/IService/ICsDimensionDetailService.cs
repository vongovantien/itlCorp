using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;

namespace eFMS.API.Documentation.DL.IService
{
    public interface ICsDimensionDetailService : IRepositoryBase<CsDimensionDetail, CsDimensionDetailModel>
    {
        HandleState UpdateMasterBill(List<CsDimensionDetailModel> dimensionDetails, Guid masterId);
        HandleState UpdateHouseBill(List<CsDimensionDetailModel> dimensionDetails, Guid housebillId);
        HandleState UpdateAirWayBill(List<CsDimensionDetailModel> dimensionDetails, Guid airbillId);
        List<CsDimensionDetailModel> GetDIMFromHouseByJob(Guid id);
    }
}
