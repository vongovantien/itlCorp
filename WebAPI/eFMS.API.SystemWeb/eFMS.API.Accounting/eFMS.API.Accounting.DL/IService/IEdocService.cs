using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.SettlementPayment;
using System.Threading.Tasks;

namespace eFMS.API.Accounting.DL.IService
{
    public interface IEdocService
    {
        Task<EdocAccUpdateModel> MapAdvanceRequest(string advNo);
        Task<EdocAccUpdateModel> MapSettleCharge(string settleNo);
        Task<EdocAccUpdateModel> MapSOACharge(string soaNo);
    }
}
