using eFMS.API.ForPartner.Service.Models;
using eFMS.API.ForPartner.DL.Models;
using ITL.NetCore.Connection.BL;
using eFMS.API.ForPartner.DL.IService;
using System;
using ITL.NetCore.Connection.EF;
using eFMS.IdentityServer.DL.UserManager;
using AutoMapper;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using eFMS.API.Common.Helpers;
using Microsoft.Extensions.Options;
using eFMS.API.Common;
using Newtonsoft.Json;
using ITL.NetCore.Common;

namespace eFMS.API.ForPartner.DL.Service
{
    public class AccAccountingManagementService : RepositoryBase<AccAccountingManagement, AccAccountingManagementModel>, IAccountingManagementService
    {
        private readonly ICurrentUser currentUser;
        private readonly IContextBase<SysPartnerApi> sysPartnerApiRepository;
        private readonly IHostingEnvironment environment;
        private readonly IOptions<AuthenticationSetting> configSetting;
        private readonly IContextBase<AcctAdvancePayment> acctAdvanceRepository;

        public AccAccountingManagementService(  
            IContextBase<AccAccountingManagement> repository,
            IContextBase<SysPartnerApi> sysPartnerApiRep,
            IContextBase<AcctAdvancePayment> acctAdvanceRepo,
            IOptions<AuthenticationSetting> config,
            IHostingEnvironment env,
            IMapper mapper,
            ICurrentUser cUser
            ) : base(repository, mapper)
        {
            currentUser = cUser;
            sysPartnerApiRepository = sysPartnerApiRep;
            acctAdvanceRepository = acctAdvanceRepo;
            environment = env;
            configSetting = config;

        }

        public AccAccountingManagementModel GetById(Guid id)
        {
            AccAccountingManagement accounting = DataContext.Get(x => x.Id == id).FirstOrDefault();
            AccAccountingManagementModel result = mapper.Map<AccAccountingManagementModel>(accounting);
            return result;
        }

        public bool ValidateApiKey(string apiKey)
        {
            bool isValid = false;
            SysPartnerApi partnerAPiInfo = sysPartnerApiRepository.Get(x => x.ApiKey == apiKey).FirstOrDefault();
            if(partnerAPiInfo != null && partnerAPiInfo.Active == true && partnerAPiInfo.Environment.ToLower() == environment.EnvironmentName.ToLower())
            {
                isValid = true;
            }
            return isValid;
        }

        public bool ValidateHashString(object body, string apiKey, string hash)
        {
            bool valid = false;
            if(body != null)
            {
                string bodyString = JsonConvert.SerializeObject(body) + apiKey + configSetting.Value.PartnerShareKey;

                string eFmsHash = Md5Helper.CreateMD5(bodyString);

                if(eFmsHash == hash)
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

        public string GenerateHashStringTest(object body, string apiKey)
        {
            object data = body;
            string bodyString = JsonConvert.SerializeObject(data) + apiKey + configSetting.Value.PartnerShareKey;
            return Md5Helper.CreateMD5(bodyString);
        }

        public HandleState UpdateVoucherAdvance(VoucherAdvance model)
        {
            HandleState result = new HandleState();



            return result;
        }
    }
}
