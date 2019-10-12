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

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatChargeDefaultService:RepositoryBase<CatChargeDefaultAccount,CatChargeDefaultAccountModel>,ICatChargeDefaultAccountService
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICurrentUser currentUser;
        private readonly IContextBase<CatCharge> chargeRepository;
        public CatChargeDefaultService(IContextBase<CatChargeDefaultAccount> repository, 
            IContextBase<CatCharge> chargeRepo,
            IMapper mapper, 
            IStringLocalizer<LanguageSub> localizer, 
            ICurrentUser user) : base(repository, mapper)
        {
            stringLocalizer = localizer;
            currentUser = user;
            chargeRepository = chargeRepo;
        }

        public List<CatChargeDefaultAccountImportModel> CheckValidImport(List<CatChargeDefaultAccountImportModel> list)
        {
            //eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            var defaultAccount = DataContext.Get().ToList();
            var chargeDefaults = chargeRepository.Get().ToList();
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
                //eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
                var charges = chargeRepository.Get().ToList();
                var chargeDefaults = DataContext.Get().ToList();
                foreach(var item in data)                {
                    var charge = charges.FirstOrDefault(x => x.Code == item.ChargeCode);
                    var listChargeDefaultAcc = chargeDefaults.Where(x => x.ChargeId == charge.Id).ToList();
                    var defaultAccount = new CatChargeDefaultAccount
                    {
                        ChargeId = charge.Id,
                        Active = (item.Status==null)?false:item.Status.ToString().ToLower() == "active" ? false : true,
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
                return new HandleState();
            }
            catch(Exception ex)
            {
                return new HandleState(ex.Message);
            }
            
        }

 
    }
}
