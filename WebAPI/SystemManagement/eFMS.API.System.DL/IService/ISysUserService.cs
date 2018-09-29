using SystemManagement.DL.Models;
using ITL.NetCore.Connection.BL;
using SystemManagementAPI.Service.Models;
using SystemManagement.DL.Models.Views;
using System.Collections.Generic;
using ITL.NetCore.Common;
using System;

namespace SystemManagement.DL.Services
{
    public interface ISysUserService: IRepositoryBase<SysUser, SysUserModel>
    {
        string ToString();
        List<vw_SysUserWithRoles> GetViewUsers();
        object GetNecessaryData();
        object GetUserDetails(string id);
        string GenerateId(Guid WorkPlaceId);

        HandleState Update(vw_sysUser user);

        HandleState ResetPassword(string id);

        HandleState AddNewUser(SysUserNoRelaModel SysUser);



    }
}