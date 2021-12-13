using eFMS.API.Accounting.DL.Models.AccountPayable;
using eFMS.API.Accounting.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System.Threading.Tasks;

namespace eFMS.API.Accounting.DL.IService
{
    public interface IAccountPayableService: IRepositoryBase<AccAccountPayable, AccAccountPayableModel>
    {
        Task<HandleState> InsertAccountPayable();
        Task<HandleState> UpdateAccountPayable();
        Task<HandleState> DeleteAccountPayable();
    }
}
