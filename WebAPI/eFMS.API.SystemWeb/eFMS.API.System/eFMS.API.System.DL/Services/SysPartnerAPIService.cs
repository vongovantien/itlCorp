using AutoMapper;
using eFMS.API.Common;
using eFMS.API.System.DL.IService;
using eFMS.API.System.DL.Models;
using eFMS.API.System.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Security.Cryptography;
using System.Text;

namespace eFMS.API.System.DL.Services
{
    public class SysPartnerAPIService : RepositoryBase<SysPartnerApi, SysPartnerAPIModel>, ISysPartnerAPIService
    {
        private readonly ICurrentUser currentUser;
        private readonly IOptions<AuthenticationSetting> configSetting;
        private readonly IHostingEnvironment environment;

        public SysPartnerAPIService(
            IMapper mapper,
            IContextBase<SysPartnerApi> repository,
            IOptions<AuthenticationSetting> config,
            IHostingEnvironment env,
            ICurrentUser ICurrentUser
            ): base(repository, mapper)
        {
            currentUser = ICurrentUser;
            configSetting = config;
            environment = env;
        }

        public string GenerateAPIKey()
        {
          
            return CalculateHash(configSetting.Value.ApiKey);
        }

        public string CalculateHash(string input)
        {
            using (var algorithm = MD5.Create()) //or MD5 SHA256 etc.
            {
                var hashedBytes = algorithm.ComputeHash(Encoding.UTF8.GetBytes(input));

                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        public HandleState Add(string apiKey)
        {
            HandleState result = new HandleState();
            SysPartnerApi data = new SysPartnerApi
            {
                Id = Guid.NewGuid(),
                DatetimeCreated = DateTime.Now,
                DatetimeModified = DateTime.Now,
                Active = true,
                Environment = environment.EnvironmentName,
                ApiKey = apiKey
            };

            DataContext.Add(data, false);
            result = DataContext.SubmitChanges();

            return result;

        }
    }


}
