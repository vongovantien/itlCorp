using eFMS.API.Setting.DL.Models;
using eFMS.API.Setting.Service.Models;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Setting.DL.IService
{
    public interface IUnlockRequestApproveService : IRepositoryBase<SetUnlockRequestApprove, SetUnlockRequestApproveModel>
    {
        SetUnlockRequestApproveModel GetInfoApproveUnlockRequest(Guid id);
        List<DeniedUnlockRequestResult> GetHistoryDenied(Guid id);
    }
}
