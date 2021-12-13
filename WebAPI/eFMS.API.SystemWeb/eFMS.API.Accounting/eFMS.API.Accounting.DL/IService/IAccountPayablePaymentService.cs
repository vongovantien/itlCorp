using eFMS.API.Accounting.DL.Models.AccountPayable;
using eFMS.API.Accounting.Service.Models;
using ITL.NetCore.Connection.BL;


namespace eFMS.API.Accounting.DL.IService
{
    public interface IAccountPayablePaymentService: IRepositoryBase<AccAccountPayablePayment, AccAccountPayablePaymentModel>
    {
    }
}
