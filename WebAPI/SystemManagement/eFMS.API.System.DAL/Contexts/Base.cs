using eFMS.API.System.Service.Models;
using ITL.NetCore.Connection.EF;

namespace eFMS.API.System.Service.Contexts
{
    public class Base<T> : ContextBase<T>
        where T : class, new()
    {
        public Base(): base()
        {
            ConfigDataContext<eFMSDataContext>("Server=192.168.7.88;Database=eFMSTest;User ID=sa;Password=P@ssw0rd;");
        }
    }
}
