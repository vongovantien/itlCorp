using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.SettlementPayment;
using ITL.NetCore.Common;
using System.Threading.Tasks;

namespace eFMS.API.Accounting.DL.IService
{
    public interface IEDocService
    {
        Task<HandleState> GenerateEdocSettlement(CreateUpdateSettlementModel model);
        Task<HandleState> GenerateEdocSOA(AcctSoaModel model);
        Task<HandleState> GenerateEdocAdvance(AcctAdvancePaymentModel model);
        Task DeleteEdocByBillingNo(string billingNo);
    }
}
