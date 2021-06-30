﻿using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.Accounting;
using eFMS.API.Accounting.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;

namespace eFMS.API.Accounting.DL.IService
{
    public interface IAccountingService : IRepositoryBase<AccAccountingManagement, AccAccountingManagementModel>
    {
        List<BravoAdvanceModel> GetListAdvanceToSyncBravo(List<Guid> Ids);
        List<BravoVoucherModel> GetListVoucherToSyncBravo(List<Guid> Ids);
        List<BravoSettlementModel> GetListSettlementToSyncBravo(List<Guid> Ids);
        List<SyncModel> GetListCdNoteToSync(List<Guid> ids);
        List<SyncCreditModel> GetListCdNoteCreditToSync(List<RequestGuidTypeListModel> models);
        List<SyncModel> GetListSoaToSync(List<string> ids);
        List<SyncCreditModel> GetListSoaCreditToSync(List<RequestStringTypeListModel> models);
        List<PaymentModel> GetListInvoicePaymentToSync(List<Guid> ids);
        List<PaymentModel> GetListObhPaymentToSync(List<string> ids);
        HandleState SyncListAdvanceToBravo(List<Guid> ids, out List<Guid> data);
        HandleState SyncListSettlementToBravo(List<Guid> ids, out List<Guid> data);
        HandleState SyncListVoucherToBravo(List<Guid> ids, out List<Guid> data);
        HandleState SyncListCdNoteToAccountant(List<Guid> ids);
        HandleState SyncListSoaToAccountant(List<string> ids);
        void SendMailAndPushNotificationToAccountant(List<SyncCreditModel> syncCreditModels);
        void SendMailAndPushNotificationDebitToAccountant(List<SyncModel> syncModels);
        List<PaymentModel> GetListReceiptToAccountant(List<Guid> ids, out List<AcctReceiptSyncModel> receiptSyncs);
        List<PaymentModel> GetListReceiptAllInToAccountant(List<Guid> ids, out List<AcctReceiptSyncModel> receiptSyncs);
        HandleState SyncListReceiptToAccountant(List<Guid> ids, List<AcctReceiptSyncModel> receiptSyncs);
        bool CheckCdNoteSynced(Guid idCdNote);
        bool CheckSoaSynced(string idSoa);
        bool CheckVoucherSynced(Guid idVoucher);
    }
}
