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
using System.Text;

namespace eFMS.API.System.DL.Services
{
    public class SysGroupService : RepositoryBase<SysGroup, SysGroupModel>, ISysGroupService
    {
        private readonly IContextBase<CatDepartment> departmentRepository;
        public SysGroupService(IContextBase<SysGroup> repository, 
            IMapper mapper,
            IContextBase<CatDepartment> departmentRepo) : base(repository, mapper)
        {
            SetChildren<SysUserGroup>("Id", "GroupId");
            departmentRepository = departmentRepo;
        }

        public IQueryable<SysGroupModel> Paging(SysGroupCriteria criteria, int page, int size, out int rowsCount)
        {
            var data = Query(criteria);

            rowsCount = (data.Count() > 0) ? data.Count() : 0;
            if (size > 0)
            {
                if (page < 1)
                {
                    page = 1;
                }
                data = data.Skip((page - 1) * size).Take(size);
            }
            return data;
        }

        public IQueryable<SysGroupModel> Query(SysGroupCriteria criteria)
        {
            IQueryable<SysGroup> groups = null;
            IQueryable<CatDepartment> departments = null;
            if (criteria.All == null)
            {
                groups = DataContext.Get(x => x.Code.IndexOf(criteria.Code ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                               && x.NameEn.IndexOf(criteria.NameEN ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                               && x.NameVn.IndexOf(criteria.NameVN ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                               && x.ShortName.IndexOf(criteria.ShortName ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                        );
                departments = departmentRepository.Get(x => x.DeptNameEn.IndexOf(criteria.DepartmentName ?? "", StringComparison.OrdinalIgnoreCase) > -1);
            }
            else
            {
                groups = DataContext.Get(x => x.Code.IndexOf(criteria.Code ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                               && x.NameEn.IndexOf(criteria.NameEN ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                               && x.NameVn.IndexOf(criteria.NameVN ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                               && x.ShortName.IndexOf(criteria.ShortName ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                        );
                departments = departmentRepository.Get(x => x.DeptNameEn.IndexOf(criteria.DepartmentName ?? "", StringComparison.OrdinalIgnoreCase) > -1);
            }
            if (groups == null) return null;
            var results = groups.Join(departments, x => x.DepartmentId, y => y.Id, (x, y) => new SysGroupModel
            {
                Id = x.Id,
                Code = x.Code,
                NameEn = x.NameEn,
                NameVn = x.NameVn,
                ShortName = x.ShortName,
                DepartmentId = x.DepartmentId,
                ParentId = x.ParentId,
                ManagerId = x.ManagerId,
                UserCreated = x.UserCreated,
                UserModified = x.UserModified,
                DatetimeCreated = x.DatetimeCreated,
                DatetimeModified = x.DatetimeModified,
                Active = x.Active,
                InactiveOn = x.InactiveOn,
                DepartmentName = y.DeptNameEn
            });
            return results;
        }
    }
}
