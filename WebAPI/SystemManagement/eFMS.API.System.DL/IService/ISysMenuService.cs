using System;
using System.Linq;
using SystemManagement.DL.Models;
using System.Collections.Generic;
using ITL.NetCore.Connection.BL;
using SystemManagementAPI.Service.Models;

namespace SystemManagement.DL.Services
{
    public interface ISysMenuService : IRepositoryBase<SysMenu,SysMenuModel>
    {
        SysMenuModel GetbyID(string ID);
        List<MenuEntity> GetMenuViewModel(long? selectedParent = null);
    }
}
