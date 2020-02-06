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
    public class SysMenuService : RepositoryBase<SysMenu, SysMenuModel>, ISysMenuService
    {
        private ISysUserPermissionService userpermissionService;
        private ISysUserPermissionGeneralService permissionGeneralService;

        public SysMenuService(IContextBase<SysMenu> repository, IMapper mapper,
            ISysUserPermissionService userpermission,
            ISysUserPermissionGeneralService permissionGeneral) : base(repository, mapper)
        {
            userpermissionService = userpermission;
            permissionGeneralService = permissionGeneral;
        }

        public List<MenuUserModel> GetMenus(string userId, Guid officeId)
        {
            var permissionId = userpermissionService.Get(x => x.UserId == userId && x.OfficeId == officeId)?.FirstOrDefault()?.Id;
            if (permissionId == null) return new List<MenuUserModel>();
            var permissionMenus = permissionGeneralService.Get(x => x.UserPermissionId == permissionId && x.Access == true);
            
            var menus = DataContext.Get(x => x.Id != null);
            var userMenus = menus.Join(permissionMenus, x => x.Id, y => y.MenuId, (x, y) => x).ToList();
            var data = mapper.Map<List<MenuUserModel>>(userMenus);
            return FlatToHierarchy(data, null);
        }
        public List<MenuUserModel> FlatToHierarchy(List<MenuUserModel> data, string parentId = null)
        {
            return (from  x in data
                    where x.ParentId == parentId
                    select new MenuUserModel
                    {
                        Id = x.Id,
                        ParentId = x.ParentId,
                        NameVn = x.NameVn,
                        NameEn = x.NameEn,
                        Description = x.Description,
                        AssemplyName = x.AssemplyName,
                        Icon = x.Icon,
                        Sequence = x.Sequence,
                        Arguments = x.Arguments,
                        Route = x.Route,
                        DisplayChild = x.DisplayChild,
                        Display = x.Display,
                        OrderNumber = x.OrderNumber,
                        SubMenus = FlatToHierarchy(data, x.Id)
                    }).OrderBy(x => x.OrderNumber).ToList();
        }
    }
}
