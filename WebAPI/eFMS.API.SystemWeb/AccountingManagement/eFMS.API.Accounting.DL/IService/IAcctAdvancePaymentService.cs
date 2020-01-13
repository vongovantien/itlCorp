using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.Criteria;
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
    public interface IAcctAdvancePaymentService : IRepositoryBase<AcctAdvancePayment, AcctAdvancePaymentModel>
    {
        List<AcctAdvancePaymentResult> Paging(AcctAdvancePaymentCriteria criteria, int page, int size, out int rowsCount);

        IQueryable<AcctAdvancePaymentResult> QueryData(AcctAdvancePaymentCriteria criteria);

        AcctAdvancePaymentModel GetAdvancePaymentByAdvanceNo(string advanceNo);

        AcctAdvancePaymentModel GetAdvancePaymentByAdvanceId(Guid advanceId);

        List<AcctAdvanceRequestModel> GetGroupRequestsByAdvanceNo(string advanceNo);

        List<AcctAdvanceRequestModel> GetGroupRequestsByAdvanceId(Guid advanceId);

        List<Shipments> GetShipments();

        HandleState AddAdvancePayment(AcctAdvancePaymentModel model);

        // Kiểm tra lô hàng (JobId, HBL, MBL) đã được add trong advance payment nào hay chưa?
        bool CheckShipmentsExistInAdvancePayment(ShipmentAdvancePaymentCriteria criteria);

        HandleState DeleteAdvancePayment(string advanceNo);

        HandleState UpdateAdvancePayment(AcctAdvancePaymentModel model);

        Crystal Preview(Guid advanceId);

        Crystal Preview(AcctAdvancePaymentModel advance);

        HandleState InsertOrUpdateApprovalAdvance(AcctApproveAdvanceModel approve);

        HandleState UpdateApproval(Guid addvanceId);

        HandleState DeniedApprove(Guid advanceId, string comment);

        AcctApproveAdvanceModel GetInfoApproveAdvanceByAdvanceNo(string advanceNo);

        List<AcctAdvanceRequestModel> GetAdvancesOfShipment();
        ResultHandle UnLock(List<string> keyWords);
    }
}
