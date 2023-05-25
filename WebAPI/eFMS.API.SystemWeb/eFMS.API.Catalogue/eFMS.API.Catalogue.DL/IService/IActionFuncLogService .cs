using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;

namespace eFMS.API.Accounting.DL.IService
{
    public interface IActionFuncLogService : IRepositoryBase<SysActionFuncLog, SysActionFuncLogModel>
    {
        HandleState AddActionFuncLog(SysActionFuncLogModel model);
        HandleState UpdateActionFuncLog(SysActionFuncLogModel model);
    }
}