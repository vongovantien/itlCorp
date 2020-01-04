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
        private readonly IContextBase<CatCharge> chargeRepository;

        public CatChargeDefaultService(IContextBase<CatChargeDefaultAccount> repository, 
            ICacheServiceBase<CatChargeDefaultAccount> cacheService, 
            IMapper mapper,
            IStringLocalizer<LanguageSub> localizer,
            ICurrentUser user,
            IContextBase<CatCharge> chargeRepo) : base(repository, cacheService, mapper)
        {
            stringLocalizer = localizer;
            currentUser = user;
            chargeRepository = chargeRepo;
        }

        public List<CatChargeDefaultAccountImportModel> CheckValidImport(List<CatChargeDefaultAccountImportModel> list)
        {
            var defaultAccount = DataContext.Get().ToList();
            var charges = chargeRepository.Get().ToList();
            list.ForEach(item =>
            {
                if (string.IsNullOrEmpty(item.Type))
                {
                    item.TypeError = stringLocalizer[LanguageSub.MSG_CHARGE_DEFAULT_VOUCHER_TYPE_EMPTY];
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.ChargeCode))
                {
                    item.ChargeCodeError = stringLocalizer[LanguageSub.MSG_CHARGE_DEFAULT_CODE_EMPTY];
                    item.IsValid = false;
                }
                else
                {
                    var charge = charges.FirstOrDefault(x => x.Code.ToLower() == item.ChargeCode.ToLower());
                    if (charge == null)
                    {
                        item.ChargeCodeError = string.Format(stringLocalizer[LanguageSub.MSG_CHARGE_DEFAULT_CODE_NOT_FOUND], item.ChargeCode);
                        item.IsValid = false;
                    }
                    else
                    {
                        item.ChargeId = charge.Id;
                        if(defaultAccount.Where(x => x.ChargeId == item.ChargeId && x.Type == item.Type).GroupBy(x => x.ChargeId == item.ChargeId && x.Type == item.Type)
                            .Any(x => x.Count() > 0))
                        {
                            item.ChargeCodeError = string.Format(stringLocalizer[LanguageSub.MSG_CHARGE_DEFAULT_EXISTED], item.ChargeCode);
                            item.IsValid = false;
                        }
                        if (list.Where(x => x.ChargeId == item.ChargeId && x.Type == item.Type).GroupBy(x => x.ChargeId == item.ChargeId && x.Type == item.Type)
                           .Any(x => x.Count() > 1))
                        {
                            item.ChargeCodeError = string.Format(stringLocalizer[LanguageSub.MSG_CHARGE_DEFAULT_DUPLICATED], item.ChargeCode);
                            item.IsValid = false;
                        }
                        if(list.Where(x => x.ChargeId == item.ChargeId).GroupBy(x => x.ChargeId).Any(x => x.Count() > 3)){
                            item.ChargeCodeError = string.Format(stringLocalizer[LanguageSub.MSG_CHARGE_DEFAULT_NOT_VALID], item.ChargeCode);
                            item.IsValid = false;
                        }
                    }
                }
                if (string.IsNullOrEmpty(item.DebitAccountNo) 
                    && string.IsNullOrEmpty(item.CreditAccountNo)
                    && item.DebitVat == null
                    && item.CreditVat == null)
                {
                    item.DebitAccountNoError = stringLocalizer[LanguageSub.MSG_CHARGE_DEFAULT_ACCOUNT_DEBIT_EMPTY];
                    item.CreditAccountNoError = stringLocalizer[LanguageSub.MSG_CHARGE_DEFAULT_ACCOUNT_DEBIT_EMPTY];
                    item.CreditVatError = stringLocalizer[LanguageSub.MSG_CHARGE_DEFAULT_ACCOUNT_DEBIT_EMPTY];
                    item.DebitVatError = stringLocalizer[LanguageSub.MSG_CHARGE_DEFAULT_ACCOUNT_DEBIT_EMPTY];
                    item.IsValid = false;
                }
                if (item.DebitVat != null)
                {
                    if(item.DebitVat> 99)
                    {
                        item.DebitVatError = "Must be lower 100";
                        item.IsValid = false;
                    }
                }
                if(item.CreditVat != null)
                {
                    if(item.CreditVat > 99)
                    {
                        item.CreditVatError = "Must be lower 100";
                        item.IsValid = false;
                    }
                }
            });
            return list;
        }

        public HandleState Import(List<CatChargeDefaultAccountImportModel> data)
        {
            try
            {
                var listCharge = new List<CatChargeDefaultAccount>();
                foreach(var item in data){
                    var defaultAccount = new CatChargeDefaultAccount
                    {
                        ChargeId = item.ChargeId,
                        DebitAccountNo = item.DebitAccountNo,
                        DebitVat = item.DebitVat,
                        CreditAccountNo = item.CreditAccountNo,
                        CreditVat = item.CreditVat,
                        Type = item.Type,
                        UserCreated = currentUser.UserID,
                        UserModified = currentUser.UserID,
                        DatetimeCreated = DateTime.Now,
                        DatetimeModified = DateTime.Now,
                        Active = item.Active,
                        InactiveOn = item.InactiveOn
                    };
                    listCharge.Add(defaultAccount);
                }
                DataContext.Add(listCharge);
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
