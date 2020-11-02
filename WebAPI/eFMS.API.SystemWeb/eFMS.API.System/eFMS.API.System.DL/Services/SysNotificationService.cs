using AutoMapper;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.System.DL.Services
{
    public class SysNotificationService : RepositoryBase<SysNotifications, SysNotificationsModel>, ISysNotificationService
    {
        private readonly ICurrentUser currentUser;
        private readonly IContextBase<SysUserNotification> sysUserNotiRepository;
        public SysNotificationService(
            IContextBase<SysNotifications> repository, 
            IMapper mapper, ICurrentUser ICurrentUser,
            IContextBase<SysUserNotification> sysUserNotiRepo
            ) : base(repository, mapper)
        {
            currentUser = ICurrentUser;
            sysUserNotiRepository = sysUserNotiRepo;

            SetChildren<SysUserNotification>("Id", "NotificationId");
        }

        public HandleState AddNew(SysNotifications model)
        {
            HandleState result = new HandleState();

            SysNotifications data = new SysNotifications
            {
                Id = Guid.NewGuid(),
                Description = "Đây là thông báo từ server",
                DatetimeCreated = DateTime.Now,
                DatetimeModified = DateTime.Now,
                Title = "eFms System",
                UserCreated = currentUser.UserID,
                UserModified = currentUser.UserID
            };

            result = DataContext.Add(data, false);

            if(result.Success)
            {
                SysUserNotification userNoti1 = new SysUserNotification
                {
                    Id = Guid.NewGuid(),
                    NotitficationId = data.Id,
                    Status = "New",
                    DatetimeCreated = DateTime.Now,
                    DatetimeModified = DateTime.Now,
                    UserId = "c7f2245d-4102-46e3-8d38-6dd2af527e9c",
                };
                sysUserNotiRepository.Add(userNoti1, false);

               SysUserNotification userNoti2 = new SysUserNotification
                {
                    Id = Guid.NewGuid(),
                    NotitficationId = data.Id,
                    Status = "New",
                    DatetimeCreated = DateTime.Now,
                    DatetimeModified = DateTime.Now,
                    UserId = "925387dc-13cc-4c5e-accd-03e5fc109c3c",
                };

                sysUserNotiRepository.Add(userNoti2, false);

            }
            sysUserNotiRepository.SubmitChanges();
            DataContext.SubmitChanges();

            return result;
        }

        public List<SysNotificationsModel> GetListNotification()
        {
            List<SysNotificationsModel> results = new List<SysNotificationsModel>();

            IList<SysNotifications> list = DataContext.Get(x => x.IsClosed == null).ToList();

            if (list != null && list.Count > 0)
            {
                foreach (var item in list)
                {
                    SysNotificationsModel d = mapper.Map<SysNotificationsModel>(item);
                    results.Add(d);
                }
            }

            return results;
        }
    }
}
