using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.Models.Criteria;
using eFMS.API.System.Service.Models;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace eFMS.API.System.DL.IService
{
    public interface IUserGroupService : IRepositoryBase<SysUserGroup, SysUserGroupModel>
    {
        IQueryable<SysUserGroup> Paging(UserGroupCriteria criteria, int page, int size, Expression<Func<SysUserGroup, object>> orderByProperty, bool isAscendingOrder, out int rowsCount);
    }
}
