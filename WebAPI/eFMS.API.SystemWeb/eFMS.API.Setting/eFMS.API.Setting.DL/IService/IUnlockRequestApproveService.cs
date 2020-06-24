using eFMS.API.Setting.DL.Models;
using eFMS.API.Setting.Service.Models;
using ITL.NetCore.Common;
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
        HandleState InsertOrUpdateApproval(SetUnlockRequestApproveModel approve);
        HandleState CheckExistSettingFlow(string type, Guid? officeId);
        HandleState CheckExistUserApproval(string type, int? groupId, int? departmentId, Guid? officeId, Guid? companyId);
    }
}
