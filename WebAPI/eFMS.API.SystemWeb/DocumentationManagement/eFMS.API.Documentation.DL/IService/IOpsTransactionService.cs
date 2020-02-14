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
using System.Text;

namespace eFMS.API.Documentation.DL.IService
{
    public interface IOpsTransactionService : IRepositoryBase<OpsTransaction, OpsTransactionModel>
    {
        IQueryable<OpsTransactionModel> Query(OpsTransactionCriteria criteria);
        OpsTransactionResult Paging(OpsTransactionCriteria criteria, int page, int size, out int rowsCount);
        //OpsTransactionModel GetBy(Guid id);
        int CheckDetailPermission(Guid id);
        OpsTransaction GetDetails(Guid id);
        bool CheckAllowDelete(Guid jobId);
        Crystal PreviewCDNOte(AcctCDNoteDetailsModel model);
        HandleState ConvertClearanceToJob(OpsTransactionClearanceModel model);
        HandleState ConvertExistedClearancesToJobs(List<OpsTransactionClearanceModel> list);
        HandleState SoftDeleteJob(Guid id);
        string CheckExist(OpsTransactionModel model);
        Crystal PreviewFormPLsheet(Guid id, string currency);
        HandleState Update(OpsTransactionModel model);
    }
}
