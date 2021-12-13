using System;
using System.Threading.Tasks;
using AutoMapper;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models.AccountPayable;
using eFMS.API.Accounting.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;


namespace eFMS.API.Accounting.DL.Services
{
    public class AccountPayableService : RepositoryBase<AccAccountPayable, AccAccountPayableModel>, IAccountPayableService
    {
        public AccountPayableService(IContextBase<AccAccountPayable> repository, IMapper mapper) : base(repository, mapper)
        {
        }

        public Task<HandleState> DeleteAccountPayable()
        {
            throw new NotImplementedException();
        }


        public async Task<HandleState> InsertAccountPayable(AccAccountPayable model)
        {
            HandleState hs = new HandleState();
            hs = await DataContext.AddAsync(model);

            return hs;
        }

        public Task<HandleState> UpdateAccountPayable()
        {
            throw new NotImplementedException();
        }
    }
}
