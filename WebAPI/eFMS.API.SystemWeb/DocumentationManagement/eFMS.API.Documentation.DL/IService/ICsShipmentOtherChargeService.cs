using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Documentation.DL.IService
{
    public interface ICsShipmentOtherChargeService : IRepositoryBase<CsShipmentOtherCharge, CsShipmentOtherChargeModel>
    {
        HandleState UpdateOtherChargeMasterBill(List<CsShipmentOtherChargeModel> otherCharges, Guid jobId);
        HandleState UpdateOtherChargeHouseBill(List<CsShipmentOtherChargeModel> otherCharges, Guid hblId);
    }
}
