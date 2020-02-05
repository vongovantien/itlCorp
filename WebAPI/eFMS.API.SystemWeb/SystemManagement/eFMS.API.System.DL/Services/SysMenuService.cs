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
        public SysMenuService(IContextBase<SysMenu> repository, IMapper mapper) : base(repository, mapper)
        {
        }

        public List<MenuModel> GetMenus()
        {
            var results = new List<MenuModel>();
            var menus = DataContext.Get(x => x.Id != null).ToList();
            var data = mapper.Map<List<MenuModel>>(menus);
            return FlatToHierarchy(data, null);
        }
        public List<MenuModel> FlatToHierarchy(List<MenuModel> data, string parentId = null)
        {
            return (from  x in data
                    where x.ParentId == parentId
                    select new MenuModel
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
                        SubMenus = FlatToHierarchy(data, x.Id)
                    }).ToList();
        }
    }
}
