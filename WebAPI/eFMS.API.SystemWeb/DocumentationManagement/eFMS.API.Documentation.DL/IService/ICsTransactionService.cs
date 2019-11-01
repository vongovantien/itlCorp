using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.Service.Models;
using eFMS.API.Documentation.Service.ViewModels;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Documentation.DL.IService
{
    public interface ICsTransactionService : IRepositoryBase<CsTransaction, CsTransactionModel>
    {
        IQueryable<CsTransactionModel> Query(CsTransactionCriteria criteria);
        List<CsTransactionModel> Paging(CsTransactionCriteria criteria, int page, int size, out int rowsCount);
        CsTransactionModel GetById(Guid id);
        object AddCSTransaction(CsTransactionEditModel model);
        object ImportCSTransaction(CsTransactionEditModel model);
        HandleState UpdateCSTransaction(CsTransactionEditModel model);
        bool CheckAllowDelete(Guid jobId);
        HandleState DeleteCSTransaction(Guid jobId);
        List<object> GetListTotalHB(Guid JobId);
    }
}
