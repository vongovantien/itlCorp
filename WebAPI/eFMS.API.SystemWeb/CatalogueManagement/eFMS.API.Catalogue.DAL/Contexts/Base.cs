using eFMS.API.Catalogue.Service.Models;
using ITL.NetCore.Connection.EF;

namespace eFMS.API.Catalogue.Service.Contexts
{
    public class Base<T> : ContextBase<T>
        where T : class, new()
    {
        public Base(): base()
        {
            ConfigDataContext<eFMSDataContext>(DbHelper.DbHelper.ConnectionString);
        }
    }
}
