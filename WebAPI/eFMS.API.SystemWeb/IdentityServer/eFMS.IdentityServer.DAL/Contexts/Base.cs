using eFMS.API.Common;
using eFMS.API.Common.Globals.Configs;
using eFMS.IdentityServer.Service.Contexts;
using eFMS.IdentityServer.Service.Models;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Options;

namespace eFMS.API.System.Service.Contexts
{
    public class Base<T> : ContextBase<T>
        where T : class, new()
    {
        public Base(IOptions<ConnectionString> connectStrings) : base()
        {
            ConfigDataContext<eFMSDataContext>(connectStrings.Value.eFMSConnection);
        }
    }
}
