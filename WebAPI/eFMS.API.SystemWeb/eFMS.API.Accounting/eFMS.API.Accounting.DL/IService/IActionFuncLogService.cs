using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.IService
{
    public interface IActionFuncLogService : IRepositoryBase<SysActionFuncLog, SysActionFuncLogModel>
    {
        HandleState AddActionFuncLog(SysActionFuncLogModel model);
        HandleState UpdateActionFuncLog(SysActionFuncLogModel model);
    }
}
