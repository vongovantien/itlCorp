using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.SettlementPayment;

namespace eFMS.API.Accounting.DL.IService
{
    public interface IEdocService
    {
        EdocAccUpdateModel MapAdvanceRequest(AcctAdvancePaymentModel model);
        EdocAccUpdateModel MapSettleCharge(CreateUpdateSettlementModel model);
        EdocAccUpdateModel MapSOACharge(AcctSoaModel model);
    }
}
