using AutoMapper;
using AutoMapper.QueryableExtensions;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace eFMS.API.System.DL.Services
{
    public class SysUserNotificationService : RepositoryBase<SysUserNotification, SysUserNotificationModel>, ISysUserNotification
    {
        private readonly ICurrentUser currentUser;
        private readonly IContextBase<SysNotifications> sysNotificationRepository;
        public SysUserNotificationService(
            IContextBase<SysUserNotification> repository,
            IContextBase<SysNotifications> sysNotificationRepo,
            IMapper mapper) : base(repository, mapper)
        {
            sysNotificationRepository = sysNotificationRepo;
        }

        public HandleState Add(SysUserNotification sysBuModel)
        {
            throw new NotImplementedException();
        }

        public HandleState Delete(Guid id)
        {
            throw new NotImplementedException();
        }

        public IQueryable<SysUserNotificationModel> GetAll()
        {
            var data = DataContext.Get();
            return data.ProjectTo<SysUserNotificationModel>(mapper.ConfigurationProvider);
        }

        public HandleState Update(Guid id, SysUserNotification sysUserNotification)
        {
            throw new NotImplementedException();
        }

        IQueryable<SysUserNotificationModel> ISysUserNotification.Paging(int page, int size, out int rowsCount)
        {
            IQueryable<SysUserNotificationModel> data = GetQuery();

            rowsCount = data.Count();
            if (page == 0)
            {
                page = 1;
                size = rowsCount;
            }
            return data.Skip((page - 1) * size).Take(size);
        }

        public IQueryable<SysUserNotificationModel> GetQuery()
        {
            Expression<Func<SysUserNotificationModel, bool>> query = q => true;

            query = (x => x.UserId == currentUser.UserID); // Lấy ra notifi của user đó

            IQueryable<SysUserNotificationModel> dataQuery = Get(query);

            var queryNotiDetail = from u in dataQuery
                                  join s in sysNotificationRepository.Get() on u.NotitficationId equals s.Id
                            
            dataQuery = dataQuery?.OrderByDescending(x => x.DatetimeModified);

            return dataQuery;


        }
    }
}
