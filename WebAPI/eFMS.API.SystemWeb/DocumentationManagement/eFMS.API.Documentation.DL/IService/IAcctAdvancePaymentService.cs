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
        List<Shipments> GetShipments();

        HandleState AddAdvancePayment(AcctAdvancePaymentModel model);

        // Kiểm tra lô hàng (JobId, HBL, MBL) đã được add trong advance payment nào hay chưa?
        bool CheckShipmentsExistInAdvancePayment(ShipmentAdvancePaymentCriteria criteria);
    }
}
