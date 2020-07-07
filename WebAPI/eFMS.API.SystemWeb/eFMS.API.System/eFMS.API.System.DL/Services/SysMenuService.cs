using AutoMapper;
using eFMS.API.Common.Globals;
using eFMS.API.System.DL.Common;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.ViewModels;
using eFMS.API.System.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.Caching;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using CommonData = eFMS.API.Common.Globals.CommonData;

namespace eFMS.API.System.DL.Services
{
    public class SysMenuService : RepositoryBaseCache<SysMenu, SysMenuModel>, ISysMenuService
    {
        private IContextBase<SysUserPermission> userpermissionRepository;
        private IContextBase<SysUserPermissionGeneral> permissionGeneralRepository;
        private readonly ICurrentUser currentUser;
        private readonly CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;

        public SysMenuService(
            IContextBase<SysMenu> repository, 
            ICacheServiceBase<SysMenu> cacheService, 
            IMapper mapper,
            IContextBase<SysUserPermission> userpermissionRepo,
            ICurrentUser icurrentUser,
            IContextBase<SysUserPermissionGeneral> permissionGeneralRepo) : base(repository, cacheService, mapper)
        {
            userpermissionRepository = userpermissionRepo;
            permissionGeneralRepository = permissionGeneralRepo;
            currentUser = icurrentUser;
        }

        //public SysMenuService(IContextBase<SysMenu> repository, IMapper mapper,
        //    IContextBase<SysUserPermission> userpermissionRepo,
        //    IContextBase<SysUserPermissionGeneral> permissionGeneralRepo) : base(repository, mapper)
        //{
        //    userpermissionRepository = userpermissionRepo;
        //    permissionGeneralRepository = permissionGeneralRepo;
        //}
        List<MenuUserModel> ISysMenuService.GetMenus(string userId, Guid officeId)
        {
            var permissionId = userpermissionRepository.Get(x => x.UserId == userId && x.OfficeId == officeId)?.FirstOrDefault()?.Id;
            if (permissionId == null) return new List<MenuUserModel>();
            var permissionMenus = permissionGeneralRepository.Get(x => x.UserPermissionId == permissionId && x.Access == true);

            var menus = Get(x => x.Id != null);
            var parentMenus = Get(x => x.ParentId == null).ToList();
            var userMenus = menus.Join(permissionMenus, x => x.Id, y => y.MenuId, (x, y) => x).ToList();
            userMenus.AddRange(parentMenus);
            var data = mapper.Map<List<MenuUserModel>>(userMenus);

            var results = FlatToHierarchy(data, null);
            if (results.Count > 0)
                return results.Where(x => x.SubMenus.Count > 0).ToList();
            return results;
        }
        public List<MenuUserModel> FlatToHierarchy(List<MenuUserModel> data, string parentId = null)
        {
            return (from  x in data
                    where x.ParentId == parentId
                    select new MenuUserModel
                    {
                        Id = x.Id,
                        ParentId = x.ParentId,
                        Name = currentCulture.IetfLanguageTag == "en-US" ? x.NameEn: x.NameVn,
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

        public List<CommonData> GetListService()
        {
            List<CommonData> results = new List<CommonData>();

            // Lấy ra Permisison của User
            Guid? permissionId = userpermissionRepository.Get(x => x.UserId == currentUser.UserID && x.OfficeId == currentUser.OfficeID)?.FirstOrDefault().Id;

            if (permissionId != Guid.Empty)
            {
                IQueryable<SysUserPermissionGeneral> permissionDetails = permissionGeneralRepository
                    .Get(x => x.UserPermissionId == permissionId && x.Access == true)
                    .Where(p => p.MenuId.Contains("doc") || p.MenuId.Contains("ops"));

                bool hasOpsService = permissionDetails.Any(x => x.MenuId.Contains("ops"));

                List<string> menuIds = (permissionDetails.Where(x => x.MenuId.Contains("doc"))).Select(x => x.MenuId).ToList();
                if(hasOpsService)
                {
                    menuIds.Insert(0,"ops");
                }

                if(menuIds.Count() > 0)
                {
                    foreach (string menuId in menuIds)
                    {
                        if(menuId == "ops")
                        {
                            results.Add(CustomData.Services.FirstOrDefault(x => x.Value == "CL"));
                            continue;
                        }
                        SysMenu menuDetail = DataContext.Get(x => x.Id == menuId)?.FirstOrDefault();
                        if(menuDetail != null)
                        {
                            results.Add(new CommonData
                            {
                                Value = CustomData.Services.FirstOrDefault(x => x.DisplayName == menuDetail.NameEn).Value,
                                DisplayName = menuDetail.NameEn
                            });
                        }
                    }
                }

            }
            return results;
        }
    }
}
