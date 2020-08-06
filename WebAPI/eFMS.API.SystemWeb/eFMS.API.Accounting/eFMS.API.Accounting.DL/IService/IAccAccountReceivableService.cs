using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.IService
{
    public interface IAccAccountReceivableService : IRepositoryBase<AccAccountReceivable, AccAccountReceivableModel>
    {
        HandleState AddReceivable(AccAccountReceivableModel model);
        HandleState UpdateReceivable(AccAccountReceivableModel model);
        HandleState InsertOrUpdateReceivable(ObjectReceivableModel model);
        HandleState CalculatorReceivable(CalculatorReceivableModel model);
    }
}
