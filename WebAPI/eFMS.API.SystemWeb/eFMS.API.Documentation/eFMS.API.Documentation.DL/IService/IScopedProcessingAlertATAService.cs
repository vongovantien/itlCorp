using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System.Threading;
using System.Threading.Tasks;

namespace eFMS.API.Documentation.DL.IService
{
    public interface IScopedProcessingAlertATAService : IRepositoryBase<CsTransaction, CsTransactionModel>
    {
        Task AlertATA();
    }
}
