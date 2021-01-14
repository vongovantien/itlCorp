using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.Models.Criteria;
using eFMS.API.System.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.System.DL.IService
{
    public interface ISysUserLevelService : IRepositoryBase<SysUserLevel, SysUserLevelModel>
    {
        IQueryable<SysUserLevelModel> GetByLevel(short LevelId);
        SysUserLevelModel GetDetail(int id);
        HandleState AddUser(List<SysUserLevelModel> users);
        IQueryable<SysUserLevelModel> Query(SysUserLevelCriteria criteria);
        IQueryable<SysUserLevelModel> GetByUserId(string id);
        IQueryable<SysUserLevelModel> GetUsersByType(UserLevelCriteria criteria);
        List<SysUserLevelModel> GetListUsersByCurrentCompany(SysUserLevelModel model);
    }
}
