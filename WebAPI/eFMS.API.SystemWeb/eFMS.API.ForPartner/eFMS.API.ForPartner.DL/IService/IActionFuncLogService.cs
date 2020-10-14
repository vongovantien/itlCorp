using eFMS.API.ForPartner.DL.Models;
using eFMS.API.ForPartner.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;

namespace eFMS.API.ForPartner.DL.IService
{
    public interface IActionFuncLogService : IRepositoryBase<SysActionFuncLog, SysActionFuncLogModel>
    {
        HandleState AddActionFuncLog(SysActionFuncLogModel model);
        HandleState UpdateActionFuncLog(SysActionFuncLogModel model);
    }
}
