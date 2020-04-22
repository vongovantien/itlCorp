using eFMS.API.Common;
using eFMS.API.Setting.Service.Contexts;
using eFMS.API.Setting.Service.Models;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Options;

namespace eFMS.API.Catalogue.Service.Contexts
{
    public class Base<T> : ContextBase<T>
        where T : class, new()
    {
        public Base(IOptions<Settings> settings) : base()
        {
            ConfigDataContext<eFMSDataContext>(settings.Value.eFMSConnection);
        }
    }
}
