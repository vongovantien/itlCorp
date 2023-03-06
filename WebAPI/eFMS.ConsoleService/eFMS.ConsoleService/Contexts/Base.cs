using eFMS.API.Common;
using eFMS.API.Infrastructure.NoSql;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.ConsoleService.Contexts
{
    public class Base<T> : ContextBase<T>
        where T : class, new()
    {
        public Base(IOptions<Settings> settings, ICurrentUser currUser) : base()
        {
            ChangeTrackerHelper.currentUser = currUser;
            /// ConfigDataContext<eFMSDataContext>(settings.Value.eFMSConnection);
            DbHelper.ConnectionString = settings.Value.eFMSConnection;
            DbHelper.MongoDBConnectionString = settings.Value.MongoConnection;
        }
    }

    public class DbHelper
    {
        public static string ConnectionString { get; set; }
        public static string MongoDBConnectionString { get; set; }
       
    }

    public class ConnectionStrings
    {
        public string eFMSConnection { get; set; }
        public string Redis { get; set; }
        public string MongoConnection { get; set; }
        public string Rabbit { get; set; }
    }
}
