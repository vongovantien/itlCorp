﻿using eFMS.API.System.DL.Models;
using eFMS.API.System.DL.ViewModels;
using eFMS.API.System.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.System.DL.IService
{
    public interface ISysUserPermissionService : IRepositoryBase<SysUserPermission, SysUserPermissionModel>
    {
        SysUserPermissionModel GetBy(string userId, Guid officeId);
        SysUserPermissionModel Get(Guid id);
        HandleState Add(List<SysUserPermissionEditModel> list);
        HandleState Delete(Guid id);
        HandleState Update(SysUserPermissionModel entity);
        IQueryable<SysUserPermissionModel> GetByUserId(string id);

    }
}
