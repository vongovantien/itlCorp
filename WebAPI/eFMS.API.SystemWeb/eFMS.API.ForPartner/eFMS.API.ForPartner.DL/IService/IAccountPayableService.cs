using eFMS.API.ForPartner.DL.Models;
using eFMS.API.ForPartner.DL.Models.Payable;
using eFMS.API.ForPartner.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System.Threading.Tasks;

namespace eFMS.API.ForPartner.DL.IService
{
    public interface IAccountPayableService: IRepositoryBase<AccAccountPayable, AccountPayableModel>
    {
        Task<HandleState> InsertAccPayable(VoucherSyncCreateModel model);
        bool IsPayableHasPayment(VoucherSyncDeleteModel model);
    }
}
