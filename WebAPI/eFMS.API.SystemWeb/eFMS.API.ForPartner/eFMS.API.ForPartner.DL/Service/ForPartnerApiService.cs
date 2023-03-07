using eFMS.API.Common;
using eFMS.API.Common.Helpers;
using eFMS.API.ForPartner.DL.IService;
using eFMS.API.ForPartner.Service.Models;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Options;
using System.Linq;

namespace eFMS.API.ForPartner.DL.Service
{
    public class ForPartnerApiService : IForPartnerApiService
    {
        private readonly IContextBase<SysPartnerApi> sysPartnerApiRepository;

        private readonly IOptions<AuthenticationSetting> configSetting;
        public ForPartnerApiService(IContextBase<SysPartnerApi> sysPartnerApiRepo, IOptions<AuthenticationSetting> config)
        {
            sysPartnerApiRepository = sysPartnerApiRepo;
            configSetting = config;
        }
        public bool ValidateApiKey(string apiKey)
        {
            bool isValid = false;
            SysPartnerApi partnerAPiInfo = sysPartnerApiRepository.Get(x => x.ApiKey == apiKey).FirstOrDefault();
            if (partnerAPiInfo != null && partnerAPiInfo.Active == true)
            {
                isValid = true;
            }
            return isValid;
        }

        public bool ValidateHashString(object body, string apiKey, string hash)
        {
            bool valid = false;
            if (body != null)
            {
                string bodyString = apiKey + configSetting.Value.PartnerShareKey;

                string eFmsHash = Md5Helper.CreateMD5(bodyString.ToLower());

                if (eFmsHash.ToLower() == hash.ToLower())
                {
                    valid = true;
                }
                else
                {
                    valid = false;
                }
            }

            return valid;
        }
    }
}
