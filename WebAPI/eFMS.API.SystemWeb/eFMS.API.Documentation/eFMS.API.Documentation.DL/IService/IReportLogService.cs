using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;

namespace eFMS.API.Documentation.DL.IService
{
    public interface IReportLogService : IRepositoryBase<SysReportLog, SysReportLogModel>
    {
        HandleState AddNew(SysReportLogModel model);
    }
}
