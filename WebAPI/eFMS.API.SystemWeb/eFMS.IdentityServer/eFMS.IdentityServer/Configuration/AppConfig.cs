using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.IdentityServer.Configuration
{
    public class AppConfig : IAppConfig
    {
        //private IConfiguration _configuration;
            
        public AppConfig(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder().SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();

            loadConfig();
        }

        private void loadConfig()
        {
            ConnectString = Configuration.GetConnectionString("eFMSConnection");
            AuthConfig = new AuthConfig()
            {
                AccessTokenLifetime = int.Parse(Configuration.GetSection("Auth:AccessTokenLifetime").Value),
                Issuer = Configuration.GetSection("Auth:Issuer").Value,
                RedirectUris = Configuration.GetSection("Auth:RedirectUris").GetChildren().Select(sl => sl.Value).ToArray(),
                RequireHttps = bool.Parse(Configuration.GetSection("Auth:RequireHttpsMetadata").Value),
                SlidingRefreshTokenLifetime = int.Parse(Configuration.GetSection("Auth:SlidingRefreshTokenLifetime").Value)
            };
            CrosConfig = new CrosConfig()
            {
                Urls = Configuration.GetSection("CrosOptions:Urls").GetChildren().Select(sl => sl.Value).ToArray()
            };

        }
        public IConfiguration Configuration { get; }
        public string ConnectString { get; private set; }

        public AuthConfig AuthConfig { get; private set; }

        public CrosConfig CrosConfig { get; private set; }
    }
}
