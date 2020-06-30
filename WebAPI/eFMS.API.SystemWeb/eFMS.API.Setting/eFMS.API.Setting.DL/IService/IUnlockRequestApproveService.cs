using eFMS.API.Setting.DL.Models;
using eFMS.API.Setting.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using eFMS.IdentityServer.DL.UserManager;

namespace eFMS.API.Setting.DL.IService
{
    public interface IUnlockRequestApproveService : IRepositoryBase<SetUnlockRequestApprove, SetUnlockRequestApproveModel>
    {
        SetUnlockRequestApproveModel GetInfoApproveUnlockRequest(Guid id);
        List<DeniedUnlockRequestResult> GetHistoryDenied(Guid id);
        HandleState InsertOrUpdateApproval(SetUnlockRequestApproveModel approve);
        HandleState CheckExistSettingFlow(string type, Guid? officeId);
        HandleState CheckExistUserApproval(string type, int? groupId, int? departmentId, Guid? officeId, Guid? companyId);
        HandleState UpdateApproval(Guid id);
        HandleState DeniedApprove(Guid id, string comment);
        HandleState CancelRequest(Guid id);
        bool CheckUserIsApproved(ICurrentUser userCurrent, SetUnlockRequest unlockRequest, SetUnlockRequestApprove approve);
        bool CheckUserIsManager(ICurrentUser userCurrent, SetUnlockRequest unlockRequest, SetUnlockRequestApprove approve);
        bool CheckIsShowBtnDeny(ICurrentUser userCurrent, SetUnlockRequest unlockRequest, SetUnlockRequestApprove approve);
    }
}
