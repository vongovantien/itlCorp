﻿using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.Documentation.DL.IService
{
    public interface IOpsTransactionService : IRepositoryBase<OpsTransaction, OpsTransactionModel>
    {
        IQueryable<OpsTransactionModel> Query(OpsTransactionCriteria criteria);
        OpsTransactionResult Paging(OpsTransactionCriteria criteria, int page, int size, out int rowsCount);
        //OpsTransactionModel GetBy(Guid id);
        int CheckDetailPermission(Guid id);
        OpsTransactionModel GetDetails(Guid id);
        bool CheckAllowDelete(Guid jobId);
        bool CheckAllowDeleteJobUsed(Guid jobId);
        //HandleState ConvertClearanceToJob(OpsTransactionClearanceModel model);
        HandleState ConvertClearanceToJob(CustomsDeclarationModel model);
        //HandleState ConvertExistedClearancesToJobs(List<OpsTransactionClearanceModel> list);
        HandleState ConvertExistedClearancesToJobs(List<CustomsDeclarationModel> list);
        HandleState SoftDeleteJob(Guid id);
        string CheckExist(OpsTransactionModel model, string mblNo, string hblNo);
        Crystal PreviewFormPLsheet(Guid id, string currency);
        HandleState Update(OpsTransactionModel model);
        ResultHandle CheckAllowConvertJob(List<CustomsDeclarationModel> list);
        HandleState LockOpsTransaction(Guid jobId);
        ResultHandle ImportDuplicateJob(OpsTransactionModel model, out List<Guid> surchargeIds);
        HandleState UpdateSurchargeOfHousebill(OpsTransactionModel model);
        int CheckUpdateMBL(OpsTransactionModel model, out string mblNo, out List<string> advs);
        ResultHandle ChargeFromReplicate(out List<Guid> Ids);
        Task<HandleState> ReplicateJobs(ReplicateIds model);
        ResultHandle AutoRateReplicate();
    }
}
