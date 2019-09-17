using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.DL.Models.SettlementPayment;
using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;

namespace eFMS.API.Documentation.DL.IService
{
    public interface IAcctSettlementPaymentService : IRepositoryBase<AcctSettlementPayment, AcctSettlementPaymentModel>
    {
        List<AcctSettlementPaymentResult> Paging(AcctSettlementPaymentCriteria criteria, int page, int size, out int rowsCount);

        List<ShipmentOfSettlementResult> GetShipmentOfSettlements(string settlementNo);

        HandleState DeleteSettlementPayment(string settlementNo);

        AcctSettlementPaymentModel GetSettlementPaymentById(Guid idSettlement);

        List<ShipmentSettlement> GetListShipmentSettlementBySettlementNo(string settlementNo);

        List<AdvancePaymentMngt> GetAdvancePaymentMngts(string JobId, string MBL, string HBL);

        List<SettlementPaymentMngt> GetSettlementPaymentMngts(string JobId, string MBL, string HBL);

        List<ShipmentChargeSettlement> GetExistsCharge(string JobId, string HBL, string MBL);

        List<ShipmentChargeSettlement> GetListShipmentChargeSettlementNoGroup(string settlementNo);

        bool CheckDuplicateShipmentSettlement(CheckDuplicateShipmentSettlementCriteria criteria);

        HandleState AddSettlementPayment(CreateUpdateSettlementModel model);

        HandleState UpdateSettlementPayment(CreateUpdateSettlementModel model);

        HandleState InsertOrUpdateApprovalSettlement(AcctApproveSettlementModel settlement);

        HandleState UpdateApproval(Guid settlementId);

        HandleState DeniedApprove(Guid settlementId, string comment);

        AcctApproveSettlementModel GetInfoApproveSettlementBySettlementNo(string settlementNo);

        Crystal Preview(string settlementNo);
    }
}
