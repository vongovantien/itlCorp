using eFMS.IdentityServer.Service.Configuration;
using eFMS.IdentityServer.Service.Contexts;
using ITL.NetCore.Connection.EF;

namespace eFMS.API.System.Service.Contexts
{
    public class Base<T> : ContextBase<T>
        where T : class, new()
    {
        public Base() : base()
        {
            ConfigDataContext<eFMSDataContext>(new AppConfiguration().GetConnectString());
        }
    }
}
