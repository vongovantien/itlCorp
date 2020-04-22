using AutoMapper;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.ViewModels;
using eFMS.API.System.Service.Models;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.System.DL.Services
{
    public class SysPermissionSampleSpecialService : RepositoryBase<SysPermissionSampleSpecial, SysPermissionSampleSpecialModel>,
        ISysPermissionSampleSpecialService
    {
        private readonly IContextBase<SysMenu> menuRepository;
        private readonly IContextBase<SysPermissionSpecialAction> specialActionRepository;

        public SysPermissionSampleSpecialService(IContextBase<SysPermissionSampleSpecial> repository,
            IMapper mapper,
            IContextBase<SysMenu> menuRepo,
            IContextBase<SysPermissionSpecialAction> specialActionRepo) : base(repository, mapper)
        {
            menuRepository = menuRepo;
            specialActionRepository = specialActionRepo;
        }

        public List<SysPermissionSampleSpecialViewModel> GetBy(Guid? permissionId)
        {
            var actionDefaults = specialActionRepository.Get().ToList();
            var modules = actionDefaults.GroupBy(x => x.ModuleId);
            if (modules == null) return null;
            var specialPermissions = DataContext.Get(x => x.PermissionId == permissionId);
            var menus = menuRepository.Get().ToList();

            List<SysPermissionSampleSpecialViewModel> results = new List<SysPermissionSampleSpecialViewModel>();
            foreach (var item in modules)
            {
                var specialP = new SysPermissionSampleSpecialViewModel();
                var module = menus.FirstOrDefault(x => x.Id == item.Key);
                specialP.ModuleName = module?.NameEn;
                specialP.ModuleID = module?.Id;
                specialP.PermissionID = permissionId == null ? Guid.Empty : (Guid)permissionId;
                List<SysPermissionSpecialViewModel> sampleSpecials = new List<SysPermissionSpecialViewModel>();
                var actions = actionDefaults.Where(x => x.ModuleId == item.Key);
                var actionsInMenu = actions.GroupBy(x => x.MenuId);
                foreach (var actionInMenu in actionsInMenu)
                {
                    var menu = menus.FirstOrDefault(x => x.Id == actionInMenu.Key);
                    var perSpecial = new SysPermissionSpecialViewModel
                    {
                        MenuId = menu?.Id,
                        MenuName = menu?.NameEn,
                        PermissionId = permissionId == null ? Guid.Empty : (Guid)permissionId,
                        ModuleId = item.Key
                    };
                    perSpecial.PermissionSpecialActions = actions.Where(x => x.MenuId == actionInMenu.Key)
                        .Select(x => new PermissionSpecialAction
                        {
                            Id = (short)(specialPermissions.FirstOrDefault(s => s.MenuId == x.MenuId && s.ActionName == x.ActionName) != null ? (short)specialPermissions.FirstOrDefault(s => s.MenuId == x.MenuId && s.ActionName == x.ActionName).Id : 0),
                            ModuleId = x.ModuleId,
                            MenuId = x.MenuId,
                            NameEn = x.NameEn,
                            NameVn = x.NameVn,
                            ActionName = x.ActionName,
                            IsAllow = specialPermissions.FirstOrDefault(s => s.MenuId == x.MenuId && s.ActionName == x.ActionName) != null? specialPermissions.FirstOrDefault(s => s.MenuId == x.MenuId && s.ActionName == x.ActionName).IsAllow: false,
                            PermissionId = permissionId == null ? Guid.Empty : (Guid)permissionId
                        }).ToList();
                    sampleSpecials.Add(perSpecial);
                }
                specialP.SysPermissionSpecials = sampleSpecials;
                results.Add(specialP);
            }
            return results;
        }
    }
}
