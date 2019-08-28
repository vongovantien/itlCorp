using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.Common;
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
    public interface IAcctAdvancePaymentService : IRepositoryBase<AcctAdvancePayment, AcctAdvancePaymentModel>
    {
        List<AcctAdvancePaymentResult> Paging(AcctAdvancePaymentCriteria criteria, int page, int size, out int rowsCount);

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

        HandleState UpdateApproval(Guid addvanceId, string userApprove);

        HandleState DeniedApprove(Guid advanceId, string userDenie, string comment);
    }
}
