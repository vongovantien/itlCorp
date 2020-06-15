using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;

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
        //HandleState ConvertClearanceToJob(OpsTransactionClearanceModel model);
        HandleState ConvertClearanceToJob(CustomsDeclarationModel model);
        //HandleState ConvertExistedClearancesToJobs(List<OpsTransactionClearanceModel> list);
        HandleState ConvertExistedClearancesToJobs(List<CustomsDeclarationModel> list);
        HandleState SoftDeleteJob(Guid id);
        string CheckExist(OpsTransactionModel model);
        Crystal PreviewFormPLsheet(Guid id, string currency);
        HandleState Update(OpsTransactionModel model);
        IQueryable<OpsTransaction> QueryByPermission(PermissionRange range);
        ResultHandle CheckAllowConvertJob(List<CustomsDeclarationModel> list);
        HandleState LockCsTransaction(Guid jobId);

    }
}
