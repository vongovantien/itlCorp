using AutoMapper;
using eFMS.API.Common;
using eFMS.API.System.DL.Common;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.Models.Criteria;
using eFMS.API.System.DL.ViewModels;
using eFMS.API.System.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace eFMS.API.System.DL.Services
{
    public class SysPermissionSampleService : RepositoryBase<SysPermissionSample, SysPermissionSampleModel>, ISysPermissionSampleService
    {
        private readonly IContextBase<SysRole> roleRepository;
        private readonly ISysPermissionSampleGeneralService permissionSampleGeneralService;
        private readonly IContextBase<SysPermissionSampleGeneral> permissioSampleGeneralRepository;
        private readonly ISysPermissionSampleSpecialService permissionSampleSpecialService;
        private readonly IContextBase<SysPermissionSampleSpecial> permissioSampleSpecialRepository;
        private readonly IStringLocalizer stringLocalizer;

        public SysPermissionSampleService(IContextBase<SysPermissionSample> repository, 
            IMapper mapper, 
            IContextBase<SysRole> roleRepo,
            ISysPermissionSampleGeneralService perSampleGeneralService,
            IContextBase<SysPermissionSampleGeneral> permissionSampleGeneralRepo,
            ISysPermissionSampleSpecialService perSampleSpecialService,
            IContextBase<SysPermissionSampleSpecial> permissioSampleSpecialRepo,
            IStringLocalizer<LanguageSub> localizer) : base(repository, mapper)
        {
            roleRepository = roleRepo;
            permissionSampleGeneralService = perSampleGeneralService;
            permissioSampleGeneralRepository = permissionSampleGeneralRepo;
            permissionSampleSpecialService = perSampleSpecialService;
            permissioSampleSpecialRepository = permissioSampleSpecialRepo;
            stringLocalizer = localizer;
        }

        public SysPermissionSampleModel GetBy(short id)
        {
            var data = DataContext.Get(x => x.Id == id).FirstOrDefault();
            if (data == null) return null;
            var result = mapper.Map<SysPermissionSampleModel>(data);
            result.SysPermissionSampleGenerals = permissionSampleGeneralService.GetBy(id);
            result.SysPermissionSampleSpecials = permissionSampleSpecialService.GetBy(id);
            return result;
        }

        public IQueryable<SysPermissionSampleModel> Query(SysPermissionGeneralCriteria criteria)
        {
            IQueryable<SysPermissionSample> data = null;
            data = DataContext.Get(x => x.Name.IndexOf(criteria.Name ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                && (x.RoleId == criteria.RoleId || criteria.RoleId == null)
                                && (x.Active == criteria.Active || criteria.Active == null)
                            );
            if (data == null) return null;
            var roles = roleRepository.Get();
            var results = data.Join(roles, x => x.RoleId, y => y.Id, (x, y) => new SysPermissionSampleModel {
                                    Id = x.Id,
                                    Name = x.Name,
                                    RoleId = x.RoleId,
                                    Type = x.Type,
                                    Active = x.Active,
                                    RoleName = y.Name
                                    });
            return results;
        }

        public override HandleState Add(SysPermissionSampleModel entity)
        {
            var permision = mapper.Map<SysPermissionSample>(entity);
            permision.UserCreated = "admin";
            permision.DatetimeCreated = permision.DatetimeModified = DateTime.Now;
            var result = DataContext.Add(permision, false);
            if (result.Success)
            {
                var listGeneral = mapper.Map<List<SysPermissionSampleGeneral>>(entity.SysPermissionSampleGenerals);
                permissioSampleGeneralRepository.Add(listGeneral);
                var listSpecial = mapper.Map<List<SysPermissionSampleSpecial>>(entity.SysPermissionSampleSpecials);
                permissioSampleSpecialRepository.SubmitChanges();
                DataContext.SubmitChanges();
                permissioSampleGeneralRepository.SubmitChanges();
            }
            return result;
        }

        public HandleState Update(SysPermissionSampleModel entity)
        {
            var permission = mapper.Map<SysPermissionSample>(entity);
            permission.UserModified = "admin";
            permission.DatetimeModified = DateTime.Now;
            var result = DataContext.Update(permission, x => x.Id == entity.Id, false);
            if (result.Success)
            {
                var list = mapper.Map<List<SysPermissionSampleGeneral>>(entity.SysPermissionSampleGenerals);
                foreach(var item in list)
                {
                    permissioSampleGeneralRepository.Update(item, x => x.Id == item.Id, false);
                }
                foreach(var item in entity.SysPermissionSampleSpecials)
                {
                    foreach(var per in item.SysPermissionSpecials)
                    {
                        foreach(var s in per.PermissionSpecialActions)
                        {
                            var peritem = new SysPermissionSampleSpecial();
                            peritem.Id = s.Id;
                            peritem.IsAllow = s.IsAllow;
                            peritem.MenuId = s.MenuId;
                            peritem.ModuleId = s.ModuleId;
                            peritem.ActionName = s.NameEn;
                            peritem.PermissionId = entity.Id;
                            if(s.Id == 0)
                            {
                                permissioSampleSpecialRepository.Add(peritem, false);
                            }
                            else
                            {
                                permissioSampleSpecialRepository.Update(peritem, x => x.Id == entity.Id, false);
                            }
                        }
                    }
                }
                DataContext.SubmitChanges();
                permissioSampleGeneralRepository.SubmitChanges();
                permissioSampleSpecialRepository.SubmitChanges();
            }
            return result;
        }

        public HandleState Delete(short id)
        {
            try
            {
                var hs = DataContext.Delete(x => x.Id == id, false);
                if (hs.Success)
                {
                    var perGenerals = permissioSampleGeneralRepository.Get(x => x.PermissionId == id);
                    foreach(var item in perGenerals)
                    {
                        permissioSampleGeneralRepository.Delete(x => x.Id == item.Id, false);
                    }
                    var perSpecials = permissioSampleSpecialRepository.Get(x => x.PermissionId == id);
                    foreach(var item in perSpecials)
                    {
                        permissioSampleSpecialRepository.Delete(x => x.Id == item.Id, false);
                    }
                    DataContext.SubmitChanges();
                    permissioSampleGeneralRepository.SubmitChanges();
                    permissioSampleSpecialRepository.SubmitChanges();
                }
                return hs;
            }
            catch (Exception ex)
            {
                var hs = new HandleState(ex.Message);
                return hs;
            }
        }
    }
}
