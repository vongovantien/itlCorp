using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.ViewModels;
using eFMS.API.System.Service.Models;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;

namespace eFMS.API.System.DL.IService
{
    public interface ISysMenuService : IRepositoryBase<SysMenu, SysMenuModel>
    {
        List<MenuUserModel> GetMenus(string userId, Guid officeId);
    }
}
