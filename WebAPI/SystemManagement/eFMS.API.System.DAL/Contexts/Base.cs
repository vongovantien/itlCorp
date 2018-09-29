using ITL.NetCore.Connection.EF;
using SystemManagementAPI.Service.Models;

namespace SystemManagementAPI.Service.Contexts
{
    public class Base<T> : ContextBase<T>
        where T : class, new()
    {
        public Base(): base()
        {
            ConfigDataContext<eTMSDataContext>("Server=192.168.7.88; Database=eTMSTest; User ID=sa; Password=P@ssw0rd");
        }
    }
}
