using AutoMapper;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Helpers;
using eFMS.API.ForPartner.DL.IService;
using eFMS.API.ForPartner.DL.Models;
using eFMS.API.ForPartner.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.ForPartner.DL.Service
{
    public class CatBankService : RepositoryBase<CatBank, CatBankModel>, ICatBankService
    {
        private readonly IContextBase<CatPartner> catPartnerRepository;
        private readonly IContextBase<SysPartnerApi> sysPartnerApiRepository;
        private readonly IOptions<AuthenticationSetting> configSetting;
        public CatBankService(IContextBase<CatBank> repository, IContextBase<CatPartner> catParnerRepo, IContextBase<SysPartnerApi> sysPartnerApiRepo, IOptions<AuthenticationSetting> config, IMapper mapper) : base(repository, mapper)
        {
            catPartnerRepository = catParnerRepo;
            sysPartnerApiRepository = sysPartnerApiRepo;
            configSetting = config;
        }
        public async Task<HandleState> UpdateBankInfoSyncStatus(BankStatusUpdateModel model)
        {
            try
            {
                var hs = new HandleState();
                var partner = await catPartnerRepository.Get(x => x.AccountNo == model.PartnerCode).FirstOrDefaultAsync();
                if (partner != null)
                {
                    var listBank = await DataContext.WhereAsync(x => x.PartnerId.ToString() == partner.Id);
                    foreach (var item in model.BankInfo)
                    {
                        var updateItem = listBank.FirstOrDefault(x => x.BankAccountNo == item.BankAccountno);
                        updateItem.ApproveDescription = item.Description;
                        updateItem.ApproveStatus = item.ApproveStatus;
                        hs = await DataContext.UpdateAsync(updateItem, x => x.Id == updateItem.Id);
                    }
                    return hs;
                }

                return new HandleState(LanguageSub.MSG_DATA_NOT_FOUND);
            }
            catch (Exception)
            {

                throw;
            }
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
