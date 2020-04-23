using eFMS.API.Common;
using eFMS.API.Documentation.Service.Contexts;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Options;

namespace eFMS.API.Shipment.Service.Contexts
{
    public class Base<T> : ContextBase<T>
        where T : class, new()
    {
        public Base(IOptions<Settings> settings) : base()
        {
            ConfigDataContext<eFMSDataContext>(settings.Value.eFMSConnection);
            DbHelper.DbHelper.ConnectionString = settings.Value.eFMSConnection;
            DbHelper.DbHelper.MongoDBConnectionString = settings.Value.MongoConnection;
        }
    }
}
