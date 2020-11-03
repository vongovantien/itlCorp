using eFMS.API.System.DL.Models;
using eFMS.API.System.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.System.DL.IService
{
    public interface ISysUserNotification : IRepositoryBase<SysUserNotification, SysUserNotificationModel>
    {
        IQueryable<SysUserNotificationModel> Paging(int page, int size, out int rowsCount, out int totalNoRead);
        IQueryable<SysUserNotificationModel> GetAll();
        HandleState Add(SysUserNotification sysBuModel);
        HandleState Update(Guid Id);
        HandleState Delete(Guid Id);
    }
}
