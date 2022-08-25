﻿using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.Service.Models;
using eFMS.API.ForPartner.DL.Models.Receivable;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.Documentation.DL.IService
{
    public interface ICsTransactionService : IRepositoryBase<CsTransaction, CsTransactionModel>
    {
        ResultHandle ImportMulti();
        IQueryable<CsTransactionModel> Query(CsTransactionCriteria criteria);
        IQueryable<CsTransactionModel> TakeShipments(IQueryable<CsTransactionModel> masterBills);
        List<CsTransactionModel> Paging(CsTransactionCriteria criteria, int page, int size, out int rowsCount);
        int CheckDetailPermission(Guid id);
        //CsTransactionModel GetById(Guid id);
        CsTransactionModel GetDetails(Guid id);
        object AddCSTransaction(CsTransactionEditModel model);
        ResultHandle ImportCSTransaction(CsTransactionEditModel model, out List<Guid> surchargeIds);
        HandleState UpdateCSTransaction(CsTransactionEditModel model);
        bool CheckAllowDelete(Guid jobId);
        HandleState DeleteCSTransaction(Guid jobId);
        HandleState SoftDeleteJob(Guid jobId, out List<ObjectReceivableModel> modelReceivable);
        List<object> GetListTotalHB(Guid JobId);
        Crystal PreviewSIFFormPLsheet(Guid jobId, Guid hblId, string currency);
        ResultHandle SyncHouseBills(Guid JobId,CsTransactionSyncHBLCriteria model);
        HandleState SyncShipmentByAirWayBill(Guid JobId, csTransactionSyncAirWayBill model);
        int CheckDeletePermission(Guid id);
        HandleState LockCsTransaction(Guid jobId);
        Crystal PreviewShipmentCoverPage(Guid id);
        LinkAirSeaInfoModel GetLinkASInfomation(string jobNo, string mblNo, string hblNo, string serviceName, string serviceMode);
        int CheckUpdateMBL(CsTransactionEditModel model, out string mblNo, out List<string> advs);
        Task<HandleState> CreateFileZip(FileDowloadZipModel m);
        string CheckHasHBLUpdateNominatedtoFreehand(CsTransactionEditModel model);
    }
    public class FileDowloadZipModel
    {
        public string FolderName { get; set; }
        public string ObjectId { get; set; }
        public string FileName { get; set; }
    }
}
