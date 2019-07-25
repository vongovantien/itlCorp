using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.IService
{
    public interface ICsShipmentSurchargeService : IRepositoryBase<CsShipmentSurcharge, CsShipmentSurchargeModel>
    {
        List<CsShipmentSurchargeDetailsModel> GetByHB(Guid HbID,string type);
        HandleState DeleteCharge(Guid chargeId);
        List<object> GroupChargeByHB(Guid id,string PartnerId,bool IsHouseBillID,bool getAll=false);
        List<CatPartner> GetAllParner(Guid id,bool IsHouseBillID);
        ChargeShipmentResult GetListChargeShipment(ChargeShipmentCriteria criteria);
    }
}
