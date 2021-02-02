using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Documentation.DL.IService
{
    public interface ICsShipmentSurchargeService : IRepositoryBase<CsShipmentSurcharge, CsShipmentSurchargeModel>
    {
        List<CsShipmentSurchargeDetailsModel> GetByHB(Guid hblid);
        IQueryable<CsShipmentSurchargeDetailsModel> GetByHB(Guid hbID,string type);
        HandleState DeleteCharge(Guid chargeId);
        List<GroupChargeModel> GroupChargeByHB(Guid id,string partnerId,bool isHouseBillID, string cdNoteCode);
        List<CatPartner> GetAllParner(Guid id,bool isHouseBillID);
        ChargeShipmentResult GetListChargeShipment(ChargeShipmentCriteria criteria);
        HousbillProfit GetHouseBillTotalProfit(Guid hblid);
        List<HousbillProfit> GetShipmentTotalProfit(Guid jobId);
        HandleState DeleteMultiple(List<Guid> listId);
        HandleState AddAndUpdate(List<CsShipmentSurchargeModel> list);
        IQueryable<CsShipmentSurchargeDetailsModel> GetRecentlyCharges(RecentlyChargeCriteria criteria);
        object CheckAccountReceivable(List<CsShipmentSurchargeModel> list);
        HandleState NotificationCreditTerm(List<CsShipmentSurchargeModel> list);
        HandleState NotificationExpiredAgreement(List<CsShipmentSurchargeModel> list);
        HandleState NotificationPaymenTerm(List<CsShipmentSurchargeModel> list);
        List<CsShipmentSurchargeImportModel> CheckValidImport(List<CsShipmentSurchargeImportModel> list);
        HandleState Import(List<CsShipmentSurchargeImportModel> data);
        HandleState UpdateFieldNetAmount_AmountUSD_VatAmountUSD();
    }
}
