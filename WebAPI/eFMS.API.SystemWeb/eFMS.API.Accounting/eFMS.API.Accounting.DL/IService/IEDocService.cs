using eFMS.API.Accounting.DL.Models.SettlementPayment;
using eFMS.API.Accounting.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace eFMS.API.Accounting.DL.IService
{
    public interface IEDocService 
    {
        Task<HandleState> GenerateEdoc(CreateUpdateSettlementModel model);
    }
}
