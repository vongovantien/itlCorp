using AutoMapper;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.Service.Models;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using eFMS.API.Catalogue.DL.Common;
using ITL.NetCore.Common;
using Microsoft.Extensions.Localization;
using eFMS.API.Catalogue.Service.Contexts;
using eFMS.API.Common.NoSql;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Connection.Caching;

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatChargeDefaultService: RepositoryBaseCache<CatChargeDefaultAccount,CatChargeDefaultAccountModel>,ICatChargeDefaultAccountService
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICurrentUser currentUser;
        private readonly ICatChargeService chargeService;

        public CatChargeDefaultService(IContextBase<CatChargeDefaultAccount> repository, 
            ICacheServiceBase<CatChargeDefaultAccount> cacheService, 
            IMapper mapper,
            ICatChargeService charge,
            IStringLocalizer<LanguageSub> localizer,
            ICurrentUser user) : base(repository, cacheService, mapper)
        {
            stringLocalizer = localizer;
            currentUser = user;
            chargeService = charge;
        }

        public List<CatChargeDefaultAccountImportModel> CheckValidImport(List<CatChargeDefaultAccountImportModel> list)
        {
            var defaultAccount = Get();
            var chargeDefaults = chargeService.Get();
            list.ForEach(item =>
            {
                if (string.IsNullOrEmpty(item.ChargeCode))
                {
                    item.ChargeCode = stringLocalizer[LanguageSub.MSG_CHARGE_DEFAULT_CODE_EMPTY];
                    item.IsValid = false;
                }
                else
                {
                    var charge = chargeDefaults.FirstOrDefault(x => x.Code == item.ChargeCode);
                    if (charge == null)
                    {
                        item.ChargeCode = string.Format(stringLocalizer[LanguageSub.MSG_CHARGE_DEFAULT_CODE_NOT_FOUND], item.ChargeCode);
                        item.IsValid = false;
                    }
                }

                if (string.IsNullOrEmpty(item.Type)){
                    item.Type = stringLocalizer[LanguageSub.MSG_CHARGE_DEFAULT_VOUCHER_TYPE_EMPTY];
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.DebitAccountNo.ToString()))
                {
                    item.DebitAccountNo = stringLocalizer[LanguageSub.MSG_CHARGE_DEFAULT_ACCOUNT_DEBIT_EMPTY];
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.CreditAccountNo.ToString()))
                {
                    item.CreditAccountNo = stringLocalizer[LanguageSub.MSG_CHARGE_DEFAULT_ACCOUNT_CREDIT_EMPTY];
                    item.IsValid = false;
                }
                if (item.DebitVat == null)
                {
                    item.DebitVat = -1;
                    item.IsValid = false;
                }
                if(item.CreditVat == null)
                {
                    item.CreditVat = -1;
                    item.IsValid = false;
                }
            });
            return list;
        }

        public HandleState Import(List<CatChargeDefaultAccountImportModel> data)
        {
            try
            {
                var charges = chargeService.Get();
                var chargeDefaults = Get();
                foreach(var item in data)                {
                    var charge = charges.FirstOrDefault(x => x.Code == item.ChargeCode);
                    var listChargeDefaultAcc = chargeDefaults.Where(x => x.ChargeId == charge.Id).ToList();
                    bool active = !string.IsNullOrEmpty(item.Status) && (item.Status.ToLower() == "active");
                    DateTime? inactiveDate = active == false ? (DateTime?)DateTime.Now : null;
                    var defaultAccount = new CatChargeDefaultAccount
                    {
                        ChargeId = charge.Id,
                        Active = active,
                        InactiveOn = inactiveDate,
                        UserCreated = currentUser.UserID,
                        UserModified = currentUser.UserID,
                        DatetimeCreated = DateTime.Now,                        
                        Type = item.Type,
                        DebitAccountNo = item.DebitAccountNo,
                        CreditAccountNo = item.CreditAccountNo,
                        DebitVat = item.DebitVat,
                        CreditVat = item.CreditVat                        
                    };
                    foreach(var acc in listChargeDefaultAcc)
                    {
                        if (acc.Type != defaultAccount.Type)
                        {
                            DataContext.Add(defaultAccount, false);
                        }
                    }
                }
                DataContext.SubmitChanges();
                ClearCache();
                Get();
                
                return new HandleState();
            }
            catch(Exception ex)
            {
                return new HandleState(ex.Message);
            }
            
        }

 
    }
}
