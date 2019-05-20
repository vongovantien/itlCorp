using eFMS.API.Common;
using eFMS.API.Documentation.Service.Contexts;
using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Options;

namespace eFMS.API.Shipment.Service.Contexts
{
    public class Base<T> : ContextBase<T>
        where T : class, new()
    {
        public Base(IOptions<Settings> settings) : base()
        {
            //ConfigDataContext<eFMSDataContext>("Server=192.168.7.88;Database=eFMSTest;User ID=sa;Password=P@ssw0rd;");
            ConfigDataContext<eFMSDataContext>(settings.Value.eFMSConnection);
            DbHelper.DbHelper.ConnectionString = settings.Value.eFMSConnection;
            DbHelper.DbHelper.MongoDBConnectionString = settings.Value.MongoDatabase;
        }
    }
}
