using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.Models.Criteria;
using eFMS.API.System.DL.ViewModels;
using eFMS.API.System.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.System.DL.IService
{
    public interface ISysUserService : IRepositoryBase<SysUser, SysUserModel>
    {
        IQueryable<SysUserViewModel> GetAll();
        SysUserViewModel GetUserById(string Id);

        SysUserModel GetUserModelById(string id);

        HandleState UpdateProfile(UserProfileCriteria criteria);

        IQueryable<SysUserViewModel> QueryPermission(SysUserCriteria criteria);
        HandleState Insert(SysUserModel sysUserModel);
        HandleState Update(SysUserModel sysUserModel);
        IQueryable<SysUserViewModel> Paging(SysUserCriteria criteria, int page, int size, out int? rowsCount);
        IQueryable<SysUserViewModel> Query(SysUserCriteria criteria);
        List<SysUserImportModel> CheckValidImport(List<SysUserImportModel> list);
        HandleState Import(List<SysUserViewModel> data);
        HandleState Delete(string id);



    }
}
