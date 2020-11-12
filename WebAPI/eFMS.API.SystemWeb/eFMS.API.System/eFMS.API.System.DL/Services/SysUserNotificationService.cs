using AutoMapper;
using AutoMapper.QueryableExtensions;
using eFMS.API.System.DL.Common;
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
            ICurrentUser currUser,
            IMapper mapper) : base(repository, mapper)
        {
            sysNotificationRepository = sysNotificationRepo;
            currentUser = currUser;
        }

        public HandleState Add(SysUserNotification sysBuModel)
        {
            throw new NotImplementedException();
        }


        public IQueryable<SysUserNotificationModel> GetAll()
        {
            var data = DataContext.Get();
            return data.ProjectTo<SysUserNotificationModel>(mapper.ConfigurationProvider);
        }

        public HandleState Update(Guid Id)
        {
            HandleState result = new HandleState();
            try
            {
                SysUserNotification data = DataContext.Get(x => x.Id == Id)?.FirstOrDefault();
                if (data == null)
                {
                    return result;
                }

                data.Status = SystemConstants.NOTIFICATION_STATUS_READ;
                data.DatetimeModified = DateTime.Now;
                data.UserModified = currentUser.UserID;

                result = DataContext.Update(data, x => x.Id == Id);

                return result;
            }
            catch (Exception)
            {
                return result;
            }
        }

        IQueryable<SysUserNotificationModel> ISysUserNotification.Paging(int page, int size, out int rowsCount, out int totalNoRead)
        {
            IQueryable<SysUserNotificationModel> data = GetQuery();

            rowsCount = data.Count();
            totalNoRead = data.Count(x => x.Status == SystemConstants.NOTIFICATION_STATUS_NEW);
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

            if (dataQuery.Count() > 0)
            {
                IQueryable<SysUserNotificationModel> queryNotiDetail = from u in dataQuery
                                                                       join s in sysNotificationRepository.Get() on u.NotitficationId equals s.Id into sGrps
                                                                       from s in sGrps.DefaultIfEmpty()
                                                                       select new SysUserNotificationModel
                                                                       {
                                                                           Title = s.Title,
                                                                           Type = s.Type,
                                                                           Action = s.Action,
                                                                           ActionLink = s.ActionLink,
                                                                           Description = s.Description,
                                                                           Id = u.Id,
                                                                           Status = u.Status,
                                                                           UserId = u.UserId,
                                                                           NotitficationId = s.Id,
                                                                           DatetimeCreated = u.DatetimeCreated,
                                                                           DatetimeModified = u.DatetimeModified,
                                                                       };

                queryNotiDetail = queryNotiDetail?.OrderByDescending(x => x.Status == SystemConstants.NOTIFICATION_STATUS_NEW);
                return queryNotiDetail;
            }
            return dataQuery;
        }
    }
}
