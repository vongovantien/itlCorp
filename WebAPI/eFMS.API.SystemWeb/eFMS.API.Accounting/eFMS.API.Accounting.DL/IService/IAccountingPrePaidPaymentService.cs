using eFMS.API.Accounting.DL.Models;
using System.Linq;

namespace eFMS.API.Accounting.DL.IService
{
    public interface IAccountingPrePaidPaymentService
    {
        IQueryable<AccPrePaidPaymentResult> Paging(AccountingPrePaidPaymentCriteria criteria, int page, int size, out int rowsCount);
    }

}
