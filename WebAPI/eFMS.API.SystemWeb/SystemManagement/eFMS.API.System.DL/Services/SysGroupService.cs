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

        public IQueryable<SysGroupModel> Query(SysGroupCriteria criteria)
        {
            if (criteria.All == null)
            {
                var groups = DataContext.Get(x => x.Code.IndexOf(criteria.Code ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                               && x.NameEn.IndexOf(criteria.NameEN ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                               && x.NameVn.IndexOf(criteria.NameVN ?? "", StringComparison.OrdinalIgnoreCase) >  -1
                                               && x.ShortName.IndexOf(criteria.ShortName ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                        );
                var departments = departmentRepository.Get(x => x.DeptNameEn.IndexOf(criteria.DepartmentName ?? "", StringComparison.OrdinalIgnoreCase) > -1);

            }
        }
    }
}
