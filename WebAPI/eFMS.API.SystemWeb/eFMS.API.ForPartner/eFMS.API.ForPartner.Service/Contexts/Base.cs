using eFMS.API.Common;
using eFMS.API.Infrastructure.NoSql;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Options;

namespace eFMS.API.ForPartner.Service.Contexts
{
    public class Base<T> : ContextBase<T>
        where T : class, new()
    {
        public Base(IOptions<Settings> settings, ICurrentUser currUser) : base()
        {
            ChangeTrackerHelper.currentUser = currUser;
            ConfigDataContext<eFMSDataContext>(settings.Value.eFMSConnection);
            DbHelper.DbHelper.ConnectionString = settings.Value.eFMSConnection;
            DbHelper.DbHelper.MongoDBConnectionString = settings.Value.MongoConnection;
        }
    }
}
