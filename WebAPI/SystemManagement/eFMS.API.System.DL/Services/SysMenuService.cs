using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;

using SystemManagement.DL.Models;
using SystemManagementAPI.Service.Models;

namespace SystemManagement.DL.Services
{
    public class SysMenuService : RepositoryBase<SysMenu, SysMenuModel>, ISysMenuService
    {       
        public SysMenuService(IContextBase<SysMenu> repository, IMapper mapper):base(repository, mapper)
        {           
        }

        public SysMenuModel GetbyID(string ID)
        {
            return First(t => t.Id == ID);
        }
        public List<MenuEntity> GetMenuViewModel(long? selectedParent = null)
        {
            List<MenuEntity> items = new List<MenuEntity>();

            //get all of them from DB
            var allCategorys = Get();
            //get parent categories
            IEnumerable<SysMenu> parentCategorys = allCategorys.Where(c => c.ParentId == null).OrderBy(t=>t.Sequence);

            foreach (var cat in parentCategorys)
            {
                //add the parent category to the item list

                items.Add(new MenuEntity
                {
                    ID = cat.Id,
                    ParentID = cat.ParentId ?? "",
                    Name_VN = cat.NameVn ?? "",
                    Name_EN = cat.NameEn ?? "",
                    Description = cat.Description ?? "",
                    AssemplyName = cat.AssemplyName ?? "",
                    Icon = cat.Icon,
                    Sequence = cat.Sequence,
                    Arguments = cat.Arguments ?? "",
                    ForWorkPlace = cat.ForWorkPlace ?? "",
                    ForServiceType = cat.ForServiceType ?? "",
                    Inactive = cat.Inactive??false,
                    InactiveOn = cat.InactiveOn,
                    Class = "collapse-level-1",
                    Button = "btn btn-ftl-icon btn-primary btn-plus-icon",
                    ButtionID= "btn-lv1-",
                    //ParentIDAPI=""
                });
                //now get all its children (separate Category in case you need recursion)
                GetSubTree(allCategorys.ToList(), cat, items);
            }
            return items;
        }
        private void GetSubTree(IList<SysMenuModel> allCats, SysMenu parent, IList<MenuEntity> items)
        {
            var subCats = allCats.Where(c => c.ParentId == parent.Id).OrderBy(t=>t.Sequence);
            if (subCats.Count() == 0)
            {
                items.Where(t => t.ID == parent.Id && (t.ParentID??"")!="").Select(c =>
                 {
                     c.Button = "";
                     c.Class = "collapse-level-3 hidden";
                     return items;
                 }).ToList();
            }

            foreach (var cat in subCats)
            {               
                //add this category
                items.Add(new MenuEntity
                {
                    ID = cat.Id,
                    ParentID = cat.ParentId ?? "",
                    //ParentIDAPI=parent.ParentId+parent.Id,
                    Name_VN =/* parent.NameVn + ">>" +*/ cat.NameVn,
                    Name_EN = /*parent.NameEn + ">>" + */cat.NameEn,
                    Description = cat.Description ?? "",
                    AssemplyName = cat.AssemplyName ?? "",
                    Icon =cat.Icon,
                    Sequence = cat.Sequence,
                    Arguments = cat.Arguments ?? "",
                    ForWorkPlace = cat.ForWorkPlace ?? "",
                    ForServiceType = cat.ForServiceType ?? "",
                    Inactive = cat.Inactive??false,
                    InactiveOn = cat.InactiveOn,
                    Class= "collapse-level-2 hidden",
                    Button= "btn btn-ftl-icon btn-transparent btn-ftl-collapse",
                    ButtionID= "btn-"
                });
                //recursive call in case your have a hierarchy more than 1 level deep
                GetSubTree(allCats, cat, items);
            }
        }
    }
}
