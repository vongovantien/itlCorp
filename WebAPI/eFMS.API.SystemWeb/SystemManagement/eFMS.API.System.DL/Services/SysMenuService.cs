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
            List<MenuModel> hierarcy = new List<MenuModel>();

            data.Where(x => x.ParentId == null).ToList().ForEach(x => hierarcy.Add(x)); //get parent

            data.Where(a => a.ParentId != 0).ToList().
                ForEach(a =>
                {
                    hierarcy.Where(b => b.Id == a.ParentId).ToList().ForEach(c => c.ChildLayers.Add(a)); //get childrens
                });

            return hierarcy;
        }

        private List<MenuModel> GetChildren(List<MenuModel> data, string parentId)
        {
            var sublayers = data.Where(x => x.ParentId == parentId).ToList();
            foreach(var item in sublayers)
            {
                var sublayers1 = data.Where(x => x.ParentId == item.ParentId).ToList();
            }
            return sublayers;
        }
    }
}
