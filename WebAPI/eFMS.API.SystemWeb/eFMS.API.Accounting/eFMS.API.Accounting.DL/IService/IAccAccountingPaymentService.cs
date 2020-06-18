using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.AccountingPayment;
using eFMS.API.Accounting.DL.Models.Criteria;
using eFMS.API.Accounting.Service.Models;
using ITL.NetCore.Connection.BL;
using System.Linq;

namespace eFMS.API.Accounting.DL.IService
{
    public interface IAccAccountingPaymentService : IRepositoryBase<AccAccountingPayment, AccAccountingPaymentModel>
    {
        IQueryable<AccAccountingPaymentModel> GetBy(string refNo);
        IQueryable<AccountingPaymentModel> Paging(PaymentCriteria criteria, int page, int size, out int rowsCount);
    }
}
