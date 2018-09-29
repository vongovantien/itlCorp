using SystemManagement.DL.Models;
using ITL.NetCore.Connection.BL;
using SystemManagementAPI.Service.Models;
using SystemManagement.DL.Models.Views;
using System.Collections.Generic;
using ITL.NetCore.Common;
using System;

namespace SystemManagement.DL.IService
{
    public interface ISysUserRoleService : IRepositoryBase<SysUserRole, SysUserRoleModel>    {
      
        HandleState AddUserRole(SysUserRole RoleToAdd);
        HandleState ChangeUserRoleStatus(int id,bool status);
    }
}
