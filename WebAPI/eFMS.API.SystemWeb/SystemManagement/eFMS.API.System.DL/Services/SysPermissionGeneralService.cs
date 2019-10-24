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
    public class SysPermissionGeneralService : RepositoryBase<SysPermissionGeneral, SysPermissionGeneralModel>, ISysPermissionGeneralService
    {
        private readonly IContextBase<SysRole> roleRepository;
        public SysPermissionGeneralService(IContextBase<SysPermissionGeneral> repository, IMapper mapper, IContextBase<SysRole> roleRepo) : base(repository, mapper)
        {
            roleRepository = roleRepo;
        }

        public IQueryable<SysPermissionGeneralModel> Query(SysPermissionGeneralCriteria criteria)
        {
            IQueryable<SysPermissionGeneralModel> data = null;
            if (criteria.All == null)
            {
                data = Get(x => x.Name.IndexOf(criteria.Name ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                 || (x.RoleId == criteria.RoleId || criteria.RoleId == null)
                                 || (x.Active == criteria.Active || criteria.Active == null)
                              );
            }
            else
            {
                data = Get(x => x.Name.IndexOf(criteria.All, StringComparison.OrdinalIgnoreCase) > -1
                            && (x.RoleId == criteria.RoleId || criteria.RoleId == null)  
                            && (x.Active == criteria.Active || criteria.Active == null)
                             );
            }
            if (data == null) return null;
            var roles = roleRepository.Get();
            var results = data.Join(roles, x => x.RoleId, y => y.Id, (x, y) => new SysPermissionGeneralModel {
                                    Id = x.Id,
                                    Name = x.Name,
                                    RoleId = x.RoleId,
                                    MenuId = x.MenuId,
                                    Type = x.Type,
                                    Active = x.Active,
                                    RoleName = y.Name
                                    });
            return data;
        }
    }
}
