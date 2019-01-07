using AutoMapper;
using eFMS.API.Common.Globals;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Service.Models;
using eFMS.API.Catalogue.Service.ViewModels;
using ITL.NetCore.Connection;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using eFMS.API.Catalogue.DL.Common;
using System.Linq.Expressions;
using eFMS.API.Catalogue.DL.ViewModels;
using System.Threading;
using System.Globalization;
using ITL.NetCore.Common;
using eFMS.API.Catalogue.Service.Helpers;

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatChargeDefaultService:RepositoryBase<CatChargeDefaultAccount,CatChargeDefaultAccountModel>,ICatChargeDefaultAccountService
    {
        public CatChargeDefaultService(IContextBase<CatChargeDefaultAccount> repository,IMapper mapper) : base(repository, mapper)
        {

        }

        public List<CatChargeDefaultAccountImportModel> CheckValidImport(List<CatChargeDefaultAccountImportModel> list)
        {
            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            var defaultAccount = dc.CatChargeDefaultAccount.ToList();
            list.ForEach(item =>
            {
                if (string.IsNullOrEmpty(item.ChargeCode))
                {
                    item.ChargeCode = string.Format("Charge code is not allow to empty!|wrong");
                    item.IsValid = false;
                }
                else
                {
                    var charge = dc.CatCharge.FirstOrDefault(x => x.Code == item.ChargeCode);
                    if (charge == null)
                    {
                        item.ChargeCode = string.Format("The charge with code {0} not found !|wrong", item.ChargeCode);
                        item.IsValid = false;
                    }
                }

                if (string.IsNullOrEmpty(item.Type)){
                    item.Type = string.Format("Voucher type is not allow to empty!|wrong");
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.DebitAccountNo.ToString()))
                {
                    item.DebitAccountNo = string.Format("Account debit no. is not allow to empty!|wrong");
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.CreditAccountNo.ToString()))
                {
                    item.CreditAccountNo = string.Format("Account credit no. is not allow to empty!|wrong");
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
                eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
                foreach(var item in data)                {
                    var charge = dc.CatCharge.FirstOrDefault(x => x.Code == item.ChargeCode);
                    var listChargeDefaultAcc = dc.CatChargeDefaultAccount.Where(x => x.ChargeId == charge.Id).ToList();
                    var defaultAccount = new CatChargeDefaultAccount
                    {
                        ChargeId = charge.Id,
                        Inactive = item.Status.ToString().ToLower() == "active" ? false : true,
                        UserCreated = ChangeTrackerHelper.currentUser,
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
                            dc.CatChargeDefaultAccount.Add(defaultAccount);
                        }
                    }
                }
                dc.SaveChanges();
                return new HandleState();
            }
            catch(Exception ex)
            {
                return new HandleState(ex.Message);
            }
            
        }

 
    }
}
