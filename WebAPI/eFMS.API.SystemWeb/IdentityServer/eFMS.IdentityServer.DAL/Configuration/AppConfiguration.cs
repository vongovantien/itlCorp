using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace eFMS.IdentityServer.Service.Configuration
{
    public class AppConfiguration : IAppConfiguration
    {
        private IConfigurationRoot _configuration;
        private IConfigurationRoot _configurationRoot;
        private string _connectString
        {
            get
            {
                return _configuration.GetConnectionString("eFMSConnection");
            }
        }
        public AppConfiguration()
        {
            _configurationRoot = GetConfig("appsettings.json");
            string strEnvironment = _configurationRoot.GetSection("Environment").Value;
            _configuration = GetConfig(string.Format("appsettings.{0}.json", strEnvironment));
        }
        public string GetConnectString()
        {
            return _connectString;
        }


        private IConfigurationRoot GetConfig(string strConfigFileName)
        {
            ConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            string path = Path.Combine(Directory.GetCurrentDirectory(), strConfigFileName);
            configurationBuilder.AddJsonFile(path, false);
            return configurationBuilder.Build();
        }
    }
}
