using AutoMapper;
using eFMS.API.System.DL.Infrastructure;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.Models.Criteria;
using eFMS.API.System.Service.Models;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace eFMS.API.System.DL.Services
{
    public class UserGroupService : RepositoryBase<SysUserGroup, SysUserGroupModel>, IUserGroupService
    {
        public UserGroupService(IContextBase<SysUserGroup> repository, IMapper mapper) : base(repository, mapper)
        {
            SetChildren<SysUser>("Id", "UserGroupId");
        }

        public IQueryable<SysUserGroup> Paging(UserGroupCriteria criteria, int page, int size, string orderByProperty, bool isAscendingOrder, out int rowsCount)
        {
            Expression<Func<SysUserGroup, bool>> query = x => (x.Code ?? "").Contains(criteria.Code ?? "")
                                          && (x.Decription ?? "").Contains(criteria.Decription ?? "")
                                          && (x.Name ?? "").Contains(criteria.Name ?? "")
                                          && (x.UserCreated ?? "").Contains(criteria.UserCreated ?? "")
                                          && (x.Inactive == criteria.Inactive || criteria.Inactive == null);
            if (size > 1)
            {
                if (page < 1)
                {
                    page = 1;
                }
                if (!string.IsNullOrEmpty(orderByProperty) && (isAscendingOrder || !isAscendingOrder))
                {
                    var orderBy = ExpressionExtension.CreateExpression<SysUserGroup, object>(orderByProperty);
                    return DataContext.Paging(query, page, size, orderBy, isAscendingOrder, out rowsCount);
                }
                else
                {
                    return DataContext.Paging(query, page, size, out rowsCount);
                }
            }
            else
            {
                var data = DataContext.Get(query);
                rowsCount = data.Count();
                return data;
            }

        }

        public IQueryable<SysUserGroup> Query(UserGroupCriteria criteria, string orderByProperty, bool isAscendingOrder)
        {
            Expression<Func<SysUserGroup, bool>> query = x => (x.Code ?? "").Contains(criteria.Code ?? "")
                                            && (x.Decription ?? "").Contains(criteria.Decription ?? "")
                                            && (x.Name ?? "").Contains(criteria.Name ?? "")
                                            && (x.UserCreated ?? "").Contains(criteria.UserCreated ?? "")
                                            && (x.Inactive == criteria.Inactive || criteria.Inactive == null);
            var results = DataContext.Get(query);
            if (!string.IsNullOrEmpty(orderByProperty) && (isAscendingOrder || !isAscendingOrder))
            {
                var orderBy = ExpressionExtension.CreateExpression<SysUserGroup, object>(orderByProperty);
                results = isAscendingOrder ? results.OrderBy(orderBy) : results.OrderByDescending(orderBy);
            }
            return results;
        }
    }
}
