using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.Criteria;
using eFMS.API.Accounting.DL.Models.ExportResults;
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

        IQueryable<AcctSettlementPaymentResult> GetDatas(AcctSettlementPaymentCriteria criteria);

        List<ShipmentOfSettlementResult> GetShipmentOfSettlements(string settlementNo);

        HandleState DeleteSettlementPayment(string settlementNo);

        AcctSettlementPaymentModel GetSettlementPaymentById(Guid idSettlement);

        List<ShipmentSettlement> GetListShipmentSettlementBySettlementNo(string settlementNo);

        List<AdvancePaymentMngt> GetAdvancePaymentMngts(string jobId, string mbl, string hbl);

        List<SettlementPaymentMngt> GetSettlementPaymentMngts(string jobId, string mbl, string hbl);

        List<ShipmentChargeSettlement> GetExistsCharge(ExistsChargeCriteria criteria);

        IQueryable<ShipmentChargeSettlement> GetListShipmentChargeSettlementNoGroup(string settlementNo);

        ResultModel CheckDuplicateShipmentSettlement(CheckDuplicateShipmentSettlementCriteria criteria);

        HandleState AddSettlementPayment(CreateUpdateSettlementModel model);

        HandleState UpdateSettlementPayment(CreateUpdateSettlementModel model);

        HandleState CheckExistsInfoManagerOfRequester(AcctApproveSettlementModel settlement);

        HandleState InsertOrUpdateApprovalSettlement(AcctApproveSettlementModel settlement);

        HandleState UpdateApproval(Guid settlementId);

        HandleState DeniedApprove(Guid settlementId, string comment);

        AcctApproveSettlementModel GetInfoApproveSettlementBySettlementNo(string settlementNo);

        List<DeniedInfoResult> GetHistoryDeniedSettlement(string settlementNo);

        Crystal Preview(string settlementNo);

        Crystal PreviewMultipleSettlement(List<string> settlementNos);

        List<ShipmentChargeSettlement> CopyChargeFromSettlementOldToSettlementNew(ShipmentsCopyCriteria criteria);
        LockedLogResultModel GetSettlePaymentsToUnlock(List<string> keyWords);
        HandleState UnLock(List<LockedLogModel> settlePayments);

        bool CheckDetailPermissionBySettlementNo(string settlementNo);

        bool CheckDetailPermissionBySettlementId(Guid settlementId);

        bool CheckDeletePermissionBySettlementNo(string settlementNo);

        bool CheckDeletePermissionBySettlementId(Guid settlementId);

        bool CheckUpdatePermissionBySettlementId(Guid settlementId);

        SettlementExport SettlementExport(Guid settlementId);

        List<SettlementExportGroupDefault> QueryDataSettlementExport(string[] settlementCode);

        HandleState RecallRequest(Guid settlementId);

        bool CheckIsLockedShipment(string jobNo);

        HandleState CheckExistSettingFlow(string type, Guid? officeId);

        HandleState CheckValidateMailByUserId(string userId);

        HandleState CheckExistUserApproval(string type, int? groupId, int? departmentId, Guid? officeId, Guid? companyId);     
    }
}
