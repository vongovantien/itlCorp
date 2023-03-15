using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.SettlementPayment;

namespace eFMS.API.Accounting.DL.IService
{
    public interface IEdocService
    {
        EdocAccUpdateModel MapAdvanceRequest(string advNo);
        EdocAccUpdateModel MapSettleCharge(string settleNo);
        EdocAccUpdateModel MapSOACharge(string soaNo);
    }
}
