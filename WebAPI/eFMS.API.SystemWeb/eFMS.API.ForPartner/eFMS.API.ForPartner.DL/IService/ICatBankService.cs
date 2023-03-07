using eFMS.API.ForPartner.DL.Models;
using eFMS.API.ForPartner.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System.Threading.Tasks;

namespace eFMS.API.ForPartner.DL.IService
{
    public interface ICatBankService : IRepositoryBase<CatBank, CatBankModel>, IForPartnerApiService
    {
        Task<HandleState> UpdateBankInfoSyncStatus(BankStatusUpdateModel model);
    }
}
