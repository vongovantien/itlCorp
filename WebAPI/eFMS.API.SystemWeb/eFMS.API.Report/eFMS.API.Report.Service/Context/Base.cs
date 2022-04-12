using eFMS.API.Common;
using eFMS.API.Infrastructure.NoSql;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Report.Service.Context
{
    public class Base<T> : ContextBase<T>
       where T : class, new()
    {
        public Base(IOptions<Settings> settings, ICurrentUser currUser) : base()
        {
            ChangeTrackerHelper.currentUser = currUser;
            ConfigDataContext<eFMSDataContext>(settings.Value.eFMSConnection);
            DbHelper.DBHelper.ConnectionString = settings.Value.eFMSConnection;
            DbHelper.DBHelper.MongoDBConnectionString = settings.Value.MongoConnection;
        }
    }
}
