using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Threading.Tasks;

namespace eFMS.API.Accounting.DL.IService
{
    public interface IAcctDebitManagementARService: IRepositoryBase<AcctDebitManagementAr,AcctDebitManagementArModel>
    {
        Task<HandleState> AddAndUpdate(Guid AcctMngtId);
        void DeleteDebit(Guid AcctMngtId);
    }
}
