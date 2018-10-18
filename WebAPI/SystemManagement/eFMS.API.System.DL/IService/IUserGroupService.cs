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
        IQueryable<SysUserGroupModel> Query(UserGroupCriteria criteria, string orderByProperty, bool isAscendingOrder);
        IQueryable<SysUserGroupModel> Query(string searchText, string orderByProperty, bool isAscendingOrder);
        IQueryable<SysUserGroupModel> Paging(UserGroupCriteria criteria, int page, int size, string orderByProperty, bool isAscendingOrder, out int rowsCount);
        IQueryable<SysUserGroupModel> Paging(string searchText, int page, int size, string orderByProperty, bool isAscendingOrder, out int rowsCount);
    }
}
