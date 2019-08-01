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
        private IConfiguration _configuration;
            
        public AppConfig(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder().SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            _configuration = builder.Build();

            loadConfig();
        }

        private void loadConfig()
        {
            ConnectString = _configuration.GetConnectionString("eFMSConnection");
            AuthConfig = new AuthConfig()
            {
                AccessTokenLifetime = int.Parse(_configuration.GetSection("Auth:AccessTokenLifetime").Value),
                Issuer = _configuration.GetSection("Auth:Issuer").Value,
                RedirectUris = _configuration.GetSection("Auth:RedirectUris").GetChildren().Select(sl => sl.Value).ToArray(),
                RequireHttps = bool.Parse(_configuration.GetSection("Auth:RequireHttpsMetadata").Value),
                SlidingRefreshTokenLifetime = int.Parse(_configuration.GetSection("Auth:SlidingRefreshTokenLifetime").Value)
            };
            CrosConfig = new CrosConfig()
            {
                Urls = _configuration.GetSection("CrosOptions:Urls").GetChildren().Select(sl => sl.Value).ToArray()
            };
            
        }

        public string ConnectString { get; private set; }

        public AuthConfig AuthConfig { get; private set; }

        public CrosConfig CrosConfig { get; private set; }
    }
}
