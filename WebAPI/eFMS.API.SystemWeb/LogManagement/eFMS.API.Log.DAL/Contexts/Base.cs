using eFMS.API.Log.Service.Models;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Log.Service.Contexts
{
    public class Base<T> : ContextBase<T>
       where T : class, new()
    {
        public Base() : base()
        {
            ConfigDataContext<eFMSDataContext>("Server=192.168.7.88;Database=eFMSTest;User ID=sa;Password=P@ssw0rd;");
        }
    }
}
