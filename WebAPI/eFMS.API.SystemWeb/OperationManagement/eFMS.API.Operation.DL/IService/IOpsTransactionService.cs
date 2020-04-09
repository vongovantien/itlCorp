using eFMS.API.Operation.DL.Models;
using eFMS.API.Operation.DL.Models.Criteria;
using eFMS.API.Operation.Service.Models;
using ITL.NetCore.Connection.BL;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Operation.DL.IService
{
    public interface IOpsTransactionService : IRepositoryBase<OpsTransaction, OpsTransactionModel>
    {

        OpsTransactionResult Paging(OpsTransactionCriteria criteria, int page, int size, out int rowsCount);
    }
}
