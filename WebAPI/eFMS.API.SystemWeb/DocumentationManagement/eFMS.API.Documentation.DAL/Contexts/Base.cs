using eFMS.API.Documentation.Service.Contexts;
using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Connection.EF;

namespace eFMS.API.Shipment.Service.Contexts
{
    public class Base<T> : ContextBase<T>
        where T : class, new()
    {
        public Base(): base()
        {
            //ConfigDataContext<eFMSDataContext>("Server=192.168.7.88;Database=eFMSTest;User ID=sa;Password=P@ssw0rd;");
            ConfigDataContext<eFMSDataContext>(DbHelper.DbHelper.ConnectionString);
        }
    }
}
