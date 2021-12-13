using AutoMapper;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models.AccountPayable;
using eFMS.API.Accounting.Service.Models;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;

namespace eFMS.API.Accounting.DL.Services
{
    public class AccountPayablePaymentService : RepositoryBase<AccAccountPayablePayment, AccAccountPayablePaymentModel>, IAccountPayablePaymentService
    {
        public AccountPayablePaymentService(IContextBase<AccAccountPayablePayment> repository, IMapper mapper) : base(repository, mapper)
        {
        }
    }
}
