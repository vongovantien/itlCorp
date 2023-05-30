using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace eFMS.API.Accounting.DL.IService
{
    public interface IAccAccountReceivableHostedService
    { 
        Task<HandleState> CalculatorReceivableDebitAmountAsync(List<ObjectReceivableModel> models);
        void CalculatetorReceivableOverDuePrepaidAsync();
    }
}
