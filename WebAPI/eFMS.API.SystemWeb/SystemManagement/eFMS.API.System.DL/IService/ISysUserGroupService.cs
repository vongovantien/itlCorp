using eFMS.API.System.DL.Models;
using eFMS.API.System.Service.Models;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.System.DL.IService
{
    public interface ISysUserGroupService : IRepositoryBase<SysUserGroup, SysUserGroupModel>
    {
        IQueryable<SysUserGroupModel> GetByGroup(short groupId);
    }
}
