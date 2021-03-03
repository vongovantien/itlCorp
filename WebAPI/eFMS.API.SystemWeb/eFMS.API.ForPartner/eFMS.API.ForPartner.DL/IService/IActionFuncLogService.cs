using eFMS.API.ForPartner.DL.Models;
using eFMS.API.ForPartner.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;

namespace eFMS.API.ForPartner.DL.IService
{
    public interface IActionFuncLogService : IRepositoryBase<SysActionFuncLog, SysActionFuncLogModel>
    {
        HandleState AddActionFuncLog(string funcLocal, string objectRequest, string objectResponse, string major, DateTime startDateProgress, DateTime endDateProgress);
        HandleState AddActionFuncLog(SysActionFuncLogModel model);
        HandleState UpdateActionFuncLog(SysActionFuncLogModel model);
    }
}
