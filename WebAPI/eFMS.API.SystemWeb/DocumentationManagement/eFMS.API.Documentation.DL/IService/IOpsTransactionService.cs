using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Documentation.DL.IService
{
    public interface IOpsTransactionService
    {
        IQueryable<object> Query(OpsTransactionCriteria criteria);
        List<OpsTransactionModel> Paging(OpsTransactionCriteria criteria, int page, int size, out int rowsCount);
    }
}
