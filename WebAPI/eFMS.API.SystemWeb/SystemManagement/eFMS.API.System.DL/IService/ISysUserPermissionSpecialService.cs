using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.ViewModels;
using eFMS.API.System.Service.Models;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.System.DL.IService
{
    public interface ISysUserPermissionSpecialService : IRepositoryBase<SysUserPermissionSpecial, SysUserPermissionSpecialModel>
    {
        List<SysUserPermissionSpecialViewModel> GetBy(Guid id);
        List<SysUserPermissionSpecialViewModel> GetUserGeneralSpecialDefault(Guid id);
    }
}
