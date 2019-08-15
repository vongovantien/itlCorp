using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Common.Globals.Configs
{
    public class AuthConfig
    {
        public string Issuer { get; set; }
        public bool RequireHttps { get; set; }
        public string[] RedirectUris { get; set; }
        public int AccessTokenLifetime { get; set; }
        public int SlidingRefreshTokenLifetime { get; set; }
    }
}
