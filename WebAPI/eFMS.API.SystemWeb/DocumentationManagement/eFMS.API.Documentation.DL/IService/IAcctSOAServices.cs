using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.Service.Models;
using eFMS.API.Documentation.Service.ViewModels;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Documentation.DL.IService
{
    public interface IAcctSOAServices : IRepositoryBase<AcctSoa,AcctSOAModel>
    {
        HandleState AddNewSOA(AcctSOAModel model);
        HandleState UpdateSOA(AcctSOAModel model);
    }
}
