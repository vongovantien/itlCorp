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

        public IQueryable<SysUserGroupModel> Paging(UserGroupCriteria criteria, int page, int size, string orderByProperty, bool isAscendingOrder, out int rowsCount)
        {
            Expression<Func<SysUserGroupModel, bool>> query = x => (x.Code ?? "").Contains(criteria.Code ?? "")
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
                    var orderBy = ExpressionExtension.CreateExpression<SysUserGroupModel, object>(orderByProperty);
                    return base.Paging(query, page, size, orderBy, isAscendingOrder, out rowsCount);
                }
                else
                {
                    return base.Paging(query, page, size, out rowsCount);
                }
            }
            else
            {
                var data = base.Get(query);
                rowsCount = data.Count();
                return data;
            }

        }

        public IQueryable<SysUserGroupModel> Paging(string searchText, int page, int size, string orderByProperty, bool isAscendingOrder, out int rowsCount)
        {
            Expression<Func<SysUserGroupModel, bool>> query = x => (x.Code ?? "").Contains(searchText ?? "")
                                              && (x.Decription ?? "").Contains(searchText ?? "")
                                              && (x.Name ?? "").Contains(searchText ?? "")
                                              && (x.UserCreated ?? "").Contains(searchText ?? "");
            if (size > 1)
            {
                if (page < 1)
                {
                    page = 1;
                }
                if (!string.IsNullOrEmpty(orderByProperty) && (isAscendingOrder || !isAscendingOrder))
                {
                    var orderBy = ExpressionExtension.CreateExpression<SysUserGroupModel, object>(orderByProperty);
                    return base.Paging(query, page, size, orderBy, isAscendingOrder, out rowsCount);
                }
                else
                {
                    return base.Paging(query, page, size, out rowsCount);
                }
            }
            else
            {
                var data = base.Get(query);
                rowsCount = data.Count();
                return data;
            }
        }

        public IQueryable<SysUserGroupModel> Query(UserGroupCriteria criteria, string orderByProperty, bool isAscendingOrder)
        {
            Expression<Func<SysUserGroupModel, bool>> query = x => (x.Code ?? "").Contains(criteria.Code ?? "")
                                            && (x.Decription ?? "").Contains(criteria.Decription ?? "")
                                            && (x.Name ?? "").Contains(criteria.Name ?? "")
                                            && (x.UserCreated ?? "").Contains(criteria.UserCreated ?? "")
                                            && (x.Inactive == criteria.Inactive || criteria.Inactive == null);
            var results = base.Get(query);
            if (!string.IsNullOrEmpty(orderByProperty) && (isAscendingOrder || !isAscendingOrder))
            {
                var orderBy = ExpressionExtension.CreateExpression<SysUserGroupModel, object>(orderByProperty);
                results = isAscendingOrder ? results.OrderBy(orderBy) : results.OrderByDescending(orderBy);
            }
            return results;
        }

        public IQueryable<SysUserGroupModel> Query(string searchText, string orderByProperty, bool isAscendingOrder)
        {
            Expression<Func<SysUserGroupModel, bool>> query = x => (x.Code ?? "").Contains(searchText ?? "")
                                               && (x.Decription ?? "").Contains(searchText ?? "")
                                               && (x.Name ?? "").Contains(searchText ?? "")
                                               && (x.UserCreated ?? "").Contains(searchText ?? "");
            var results = base.Get(query);
            if (!string.IsNullOrEmpty(orderByProperty) && (isAscendingOrder || !isAscendingOrder))
            {
                var orderBy = ExpressionExtension.CreateExpression<SysUserGroupModel, object>(orderByProperty);
                results = isAscendingOrder ? results.OrderBy(orderBy) : results.OrderByDescending(orderBy);
            }
            return results;
        }
    }
}
