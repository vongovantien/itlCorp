﻿using AutoMapper;
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
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.ForPartner.DL.Service
{
    public class CatPartnerBankService : RepositoryBase<CatPartnerBank, CatPartnerBankModel>, ICatPartnerBankService
    {
        private readonly IContextBase<CatPartner> catPartnerRepository;
        private readonly IContextBase<SysPartnerApi> sysPartnerApiRepository;
        private readonly IOptions<AuthenticationSetting> configSetting;
        private readonly IStringLocalizer<LanguageSub> _stringLocalizer;
        public CatPartnerBankService(IContextBase<CatPartnerBank> repository,
            IContextBase<CatPartner> catParnerRepo,
            IContextBase<SysPartnerApi> sysPartnerApiRepo,
            IOptions<AuthenticationSetting> config,
            IMapper mapper,
            IStringLocalizer<LanguageSub> stringLocalizer) : base(repository, mapper)
        {
            catPartnerRepository = catParnerRepo;
            sysPartnerApiRepository = sysPartnerApiRepo;
            configSetting = config;
            _stringLocalizer = stringLocalizer;
        }

        public async Task<HandleState> UpdatePartnerBankInfoSyncStatus(BankStatusUpdateModel model)
        {
            try
            {
                var hs = new HandleState();
                var errorsMessage = new List<string>();
                var partner = await catPartnerRepository.Get(x => x.AccountNo == model.PartnerCode).FirstOrDefaultAsync();
                var listBank = await DataContext.WhereAsync(x => x.PartnerId.ToString() == partner.Id);
                foreach (var item in model.BankInfo)
                {
                    if (listBank.Any(x => x.BankAccountNo == item.BankAccountno))
                    {
                        var updateItem = listBank.FirstOrDefault(x => x.BankAccountNo == item.BankAccountno);
                        updateItem.ApproveDescription = item.Description;
                        updateItem.ApproveStatus = item.ApproveStatus;
                        hs = await DataContext.UpdateAsync(updateItem, x => x.Id == updateItem.Id);
                    }
                    else
                    {
                        errorsMessage.Add(item.BankAccountno);
                    }
                }
                hs = new HandleState(false, (object)$"{string.Join(", ", errorsMessage)} không tồn tại trong hệ thống");
                return hs;
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
