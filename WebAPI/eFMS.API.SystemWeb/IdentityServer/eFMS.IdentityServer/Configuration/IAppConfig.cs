using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.IdentityServer.Configuration
{
    public interface IAppConfig
    {
        string ConnectString { get; }  
        AuthConfig AuthConfig { get; }
        CrosConfig CrosConfig { get; }
    }

    public class AuthConfig
    {
        public string Issuer { get; set; }
        public bool RequireHttps { get; set; }
        public string[] RedirectUris { get; set; }
        public int AccessTokenLifetime { get; set; }
        public int SlidingRefreshTokenLifetime { get; set; }
    }

    public class CrosConfig {
        public string[] Urls { get; set; }
    }
}
