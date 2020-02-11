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
using System.Text;

namespace eFMS.API.System.DL.Services
{
    public class SysUserPermissionSpecialService : RepositoryBase<SysUserPermissionSpecial, SysUserPermissionSpecialModel>, ISysUserPermissionSpecialService
    {
        private readonly IContextBase<SysMenu> menuRepository;
        private readonly IContextBase<SysPermissionSpecialAction> specialActionRepository;
        private readonly IContextBase<SysPermissionSampleSpecialModel> userPermissionSpecialRepository;
        public SysUserPermissionSpecialService(IContextBase<SysUserPermissionSpecial> repository, 
            IMapper mapper,
            IContextBase<SysPermissionSpecialAction> specialActionRepo,
            IContextBase<SysMenu> menuRepo,
            IContextBase<SysPermissionSampleSpecialModel> userPermissionSpecialRepo) : base(repository, mapper)
        {
            specialActionRepository = specialActionRepo;
            menuRepository = menuRepo;
            userPermissionSpecialRepository = userPermissionSpecialRepo;
        }

        public List<SysUserPermissionSpecialViewModel> GetBy(Guid id)
        {
            var actionDefaults = specialActionRepository.Get().ToList();
            var modules = actionDefaults.GroupBy(x => x.ModuleId);
            if (modules == null) return null;
            var specialPermissions = DataContext.Get(x => x.UserPermissionId == id);
            var menus = menuRepository.Get().ToList();

            List<SysUserPermissionSpecialViewModel> results = new List<SysUserPermissionSpecialViewModel>();
            foreach (var item in modules)
            {
                var specialP = new SysUserPermissionSpecialViewModel();
                var module = menus.FirstOrDefault(x => x.Id == item.Key);
                specialP.ModuleName = module?.NameEn;
                specialP.ModuleId = module?.Id;
                specialP.UserPermissionId = id;
                List<UserPermissionSpecialViewModel> sampleSpecials = new List<UserPermissionSpecialViewModel>();
                var actions = actionDefaults.Where(x => x.ModuleId == item.Key);
                var actionsInMenu = actions.GroupBy(x => x.MenuId);
                foreach (var actionInMenu in actionsInMenu)
                {
                    var menu = menus.FirstOrDefault(x => x.Id == actionInMenu.Key);
                    var perSpecial = new UserPermissionSpecialViewModel
                    {
                        MenuId = menu?.Id,
                        MenuName = menu?.NameEn,
                        UserPermissionId = id,
                        ModuleId = item.Key
                    };
                    perSpecial.PermissionSpecialActions = actions.Where(x => x.MenuId == actionInMenu.Key)
                        .Select(x => new UserPermissionSpecialAction
                        {
                            ModuleId = x.ModuleId,
                            MenuId = x.MenuId,
                            NameEn = x.NameEn,
                            NameVn = x.NameVn,
                            IsAllow = false,
                            UserPermissionId = id
                        }).ToList();
                    perSpecial.PermissionSpecialActions.ForEach(x => {
                        var detail = specialPermissions.FirstOrDefault(y => y.ActionName == x.NameEn);
                        if (detail != null)
                        {
                            x.Id = detail.Id;
                            x.IsAllow = detail.IsAllow;
                        }
                    });
                    sampleSpecials.Add(perSpecial);
                }
                specialP.SysPermissionSpecials = sampleSpecials;
                results.Add(specialP);
            }
            return results;
        }
    }
}
