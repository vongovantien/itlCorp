using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.ViewModels;
using eFMS.API.System.Service.Models;
using eFMS.API.System.Service.ViewModels;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.System.DL.IService
{
    public interface ISysUserService : IRepositoryBase<SysUser, SysUserModel>
    {
        List<SysUserViewModel> GetAll();
        List<vw_sysUser> GetUserWorkplace();
    }
}
