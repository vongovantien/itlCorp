using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.Models.Criteria;
using eFMS.API.System.DL.ViewModels;
using eFMS.API.System.Service.Models;
using eFMS.API.System.Service.ViewModels;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.System.DL.IService
{
    public interface ISysUserService : IRepositoryBase<SysUser, SysUserModel>
    {
        List<SysUserViewModel> GetAll();
        List<vw_sysUser> GetUserWorkplace();
        HandleState AddUser(SysUserAddModel model);
        LoginReturnModel Login(string username, string password);
        SysUserViewModel GetUserById(string Id);
        IQueryable<SysUserViewModel> Paging(SysUserCriteria criteria, int page, int size, out int rowsCount);
        IQueryable<SysUserViewModel> Query(SysUserCriteria criteria);
    }
}
