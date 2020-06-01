using AutoMapper;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Accounting.DL.Services
{
    public class AccountingManagementService : RepositoryBase<AccAccountingManagement, AccAccountingManagementModel>, IAccountingManagementService
    {
        public AccountingManagementService(IContextBase<AccAccountingManagement> repository, IMapper mapper) : base(repository, mapper)
        {
        }

        public HandleState Delete(Guid id)
        {
            var data = DataContext.Get(x => x.Id == id).FirstOrDefault();
            if(data != null)
            {
                if(data.Type == "Voucher")
                {

                }
            }
            return new HandleState();
        }
    }
}
