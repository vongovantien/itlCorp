using AutoMapper;
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
        }
        public IQueryable<SysUserGroup> Paging(UserGroupCriteria criteria, int page, int size, Expression<Func<SysUserGroup, object>> orderByProperty, bool isAscendingOrder, out int rowsCount)
        {
            var results = DataContext.Get();
            if(results != null)
            {
                results = results.Where(x => (x.Code ?? "").Contains(criteria.Code ?? "")
                                          && (x.Decription ?? "").Contains(criteria.Decription ?? "")
                                          && (x.Name ?? "").Contains(criteria.Name ?? "")
                                          && (x.UserCreated ?? "").Contains(criteria.UserCreated ?? "")
                                          && (x.Inactive == criteria.Inactive || criteria.Inactive == null)
                    );
                rowsCount = results.Count();
                if (orderByProperty != null)
                    results = isAscendingOrder
                                ? results.OrderBy(orderByProperty)
                                : results.OrderByDescending(orderByProperty);
                if(page > 0 && size > 0)
                {
                    var excludedRows = (page - 1) * size;
                    results = results.Skip(excludedRows).Take(size);
                }
            }
            else
            {
                rowsCount = 0;
            }
            return results;
        }
    }
}
