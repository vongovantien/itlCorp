using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.IdentityServer.Service.Configuration
{
    public interface IAppConfiguration
    {
        string GetConnectString();
    }
}
