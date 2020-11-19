using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.Criteria;
using eFMS.API.Accounting.DL.Models.ExportResults;
using eFMS.API.Accounting.Service.Models;
using eFMS.API.Common.Globals;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Accounting.DL.IService
{
    public interface IAcctAdvancePaymentService : IRepositoryBase<AcctAdvancePayment, AcctAdvancePaymentModel>
    {
        List<AcctAdvancePaymentResult> Paging(AcctAdvancePaymentCriteria criteria, int page, int size, out int rowsCount);

        IQueryable<AcctAdvancePaymentResult> GetDatas(AcctAdvancePaymentCriteria criteria);

        AcctAdvancePaymentModel GetAdvancePaymentByAdvanceNo(string advanceNo);

        AcctAdvancePaymentModel GetAdvancePaymentByAdvanceId(Guid advanceId);

        List<AcctAdvanceRequestModel> GetGroupRequestsByAdvanceNo(string advanceNo);

        List<AcctAdvanceRequestModel> GetGroupRequestsByAdvanceNoList(string[] advanceNoList);

        List<AcctAdvanceRequestModel> GetGroupRequestsByAdvanceId(Guid advanceId);

        List<Shipments> GetShipments();

        HandleState AddAdvancePayment(AcctAdvancePaymentModel model);

        // Kiểm tra lô hàng (JobId, HBL, MBL) đã được add trong advance payment nào hay chưa?
        bool CheckShipmentsExistInAdvancePayment(ShipmentAdvancePaymentCriteria criteria);

        HandleState DeleteAdvancePayment(string advanceNo);

        HandleState UpdateAdvancePayment(AcctAdvancePaymentModel model);

        Crystal Preview(Guid advanceId);

        Crystal PreviewMultipleAdvance(List<Guid> advanceIds);

        HandleState InsertOrUpdateApprovalAdvance(AcctApproveAdvanceModel approve);

        HandleState UpdateApproval(Guid advanceId);

        HandleState DeniedApprove(Guid advanceId, string comment);

        AcctApproveAdvanceModel GetInfoApproveAdvanceByAdvanceNo(string advanceNo);

        List<DeniedInfoResult> GetHistoryDeniedAdvance(string advanceNo);

        List<AcctAdvanceRequestModel> GetAdvancesOfShipment();

        LockedLogResultModel GetAdvanceToUnlock(List<string> keyWords);

        HandleState UnLock(List<LockedLogModel> advancePayments);

        bool CheckDetailPermissionByAdvanceNo(string advanceNo);

        bool CheckDetailPermissionByAdvanceId(Guid advanceId);

        bool CheckDeletePermissionByAdvanceNo(string advanceNo);

        bool CheckDeletePermissionByAdvanceId(Guid advanceId);

        bool CheckUpdatePermissionByAdvanceId(Guid advanceId);

        AdvanceExport AdvancePaymentExport(Guid advanceId, string language);

        void UpdateStatusPaymentOfAdvanceRequest(string settlementCode);

        HandleState RecallRequest(Guid advanceId);

        HandleState UpdatePaymentVoucher(AcctAdvancePaymentModel model);

        List<AccAdvancePaymentVoucherImportModel> CheckValidImport(List<AccAdvancePaymentVoucherImportModel> list, bool validDate);

        HandleState Import(List<AccAdvancePaymentVoucherImportModel> data);

        HandleState CheckExistSettingFlow(string type, Guid? officeId);

        HandleState CheckValidateMailByUserId(string userId);

        HandleState CheckExistUserApproval(string type, int? groupId, int? departmentId, Guid? officeId, Guid? companyId);

        List<Guid> GetSurchargeIdByHblId(Guid? hblId);

        List<AcctAdvanceRequestModel> GetAdvanceRequestByAdvanceNo(string advanceNo);

        HandleState UpdatePaymentTerm(Guid Id, decimal days);

    }
}
