using eFMS.API.System.DL.Models;
using eFMS.API.System.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System.Collections.Generic;

namespace eFMS.API.System.DL.IService
{
    public interface ISysNotificationService : IRepositoryBase<SysNotifications, SysNotificationsModel>
    {
        HandleState AddNew(SysNotifications model);
        List<SysNotificationsModel> GetListNotification();

    }
}
