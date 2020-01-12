using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.Criteria;
using eFMS.API.Accounting.DL.Models.SettlementPayment;
using eFMS.API.Accounting.Service.Models;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Accounting.DL.IService
{
    public interface IAcctSettlementPaymentService : IRepositoryBase<AcctSettlementPayment, AcctSettlementPaymentModel>
    {
        List<AcctSettlementPaymentResult> Paging(AcctSettlementPaymentCriteria criteria, int page, int size, out int rowsCount);

        IQueryable<AcctSettlementPaymentResult> QueryData(AcctSettlementPaymentCriteria criteria);

        List<ShipmentOfSettlementResult> GetShipmentOfSettlements(string settlementNo);

        HandleState DeleteSettlementPayment(string settlementNo);

        AcctSettlementPaymentModel GetSettlementPaymentById(Guid idSettlement);

        List<ShipmentSettlement> GetListShipmentSettlementBySettlementNo(string settlementNo);

        List<AdvancePaymentMngt> GetAdvancePaymentMngts(string JobId, string MBL, string HBL);

        List<SettlementPaymentMngt> GetSettlementPaymentMngts(string JobId, string MBL, string HBL);

        List<ShipmentChargeSettlement> GetExistsCharge(ExistsChargeCriteria criteria);

        IQueryable<ShipmentChargeSettlement> GetListShipmentChargeSettlementNoGroup(string settlementNo);

        bool CheckDuplicateShipmentSettlement(CheckDuplicateShipmentSettlementCriteria criteria);

        HandleState AddSettlementPayment(CreateUpdateSettlementModel model);

        HandleState UpdateSettlementPayment(CreateUpdateSettlementModel model);

        HandleState InsertOrUpdateApprovalSettlement(AcctApproveSettlementModel settlement);

        HandleState UpdateApproval(Guid settlementId);

        HandleState DeniedApprove(Guid settlementId, string comment);

        AcctApproveSettlementModel GetInfoApproveSettlementBySettlementNo(string settlementNo);

        Crystal Preview(string settlementNo);

        List<ShipmentChargeSettlement> CopyChargeFromSettlementOldToSettlementNew(ShipmentsCopyCriteria criteria);
        ResultHandle UnLock(List<string> keyWords);
    }
}
