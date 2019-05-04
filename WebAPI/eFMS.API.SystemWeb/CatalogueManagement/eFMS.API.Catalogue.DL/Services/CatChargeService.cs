using AutoMapper;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System.Linq;
using System;
using System.Collections.Generic;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.DL.Common;
using Microsoft.Extensions.Localization;
using ITL.NetCore.Connection.NoSql;

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatChargeService  :RepositoryBase<CatCharge,CatChargeModel>,ICatChargeService
    {
        private readonly IStringLocalizer stringLocalizer;
        public CatChargeService(IContextBase<CatCharge> repository,IMapper mapper, IStringLocalizer<LanguageSub> localizer) :base(repository,mapper)
        {
            stringLocalizer = localizer;
        }

        public HandleState AddCharge(CatChargeAddOrUpdateModel model)
        {
            Guid chargeId = Guid.NewGuid();
            model.Charge.Id = chargeId;
            model.Charge.Inactive = false;
            model.Charge.UserCreated = ChangeTrackerHelper.currentUser;
            model.Charge.DatetimeCreated = DateTime.Now;

            try
            {
                DataContext.Add(model.Charge);

                foreach (var x in model.ListChargeDefaultAccount)
                {
                    x.ChargeId = chargeId;
                    x.Inactive = false;
                    x.UserCreated = ChangeTrackerHelper.currentUser;
                    x.DatetimeCreated = DateTime.Now;
                    ((eFMSDataContext)DataContext.DC).CatChargeDefaultAccount.Add(x);
                    ((eFMSDataContext)DataContext.DC).SaveChanges();
                }
                var hs = new HandleState();
                return hs;
            }
            catch (Exception ex)
            {
                var hs = new HandleState(ex.Message);
                return hs;
            }
            
        }

        public HandleState UpdateCharge(CatChargeAddOrUpdateModel model)
        {
            model.Charge.UserModified = ChangeTrackerHelper.currentUser;
            model.Charge.DatetimeModified = DateTime.Now;
            try
            {
                DataContext.Update(model.Charge, x => x.Id == model.Charge.Id);
                foreach(var x in model.ListChargeDefaultAccount)
                {
                    x.UserModified = ChangeTrackerHelper.currentUser;
                    x.DatetimeModified = DateTime.Now;
                    ((eFMSDataContext)DataContext.DC).CatChargeDefaultAccount.Update(x);
                    ((eFMSDataContext)DataContext.DC).SaveChanges();
                }
                var hs = new HandleState();
                return hs;
            }
            catch(Exception ex)
            {
                var hs = new HandleState(ex.Message);
                return hs;
            }
            
        }

        public CatChargeAddOrUpdateModel GetChargeById(Guid id)
        {
            CatChargeAddOrUpdateModel returnCharge = new CatChargeAddOrUpdateModel();
            var charge = DataContext.Get(x => x.Id == id).FirstOrDefault();
            var listChargeDefault = ((eFMSDataContext)DataContext.DC).CatChargeDefaultAccount.Where(x => x.ChargeId == id).ToList();
            returnCharge.Charge = charge;
            returnCharge.ListChargeDefaultAccount = listChargeDefault;
            return returnCharge;
        }



        public List<object> GetCharges(CatChargeCriteria criteria, int page, int size, out int rowsCount)
        {
            var list = Query(criteria);
            List<object> listReturn = new List<object>();
            if(criteria.Type!=null && criteria.ServiceTypeId!=null && criteria.Inactive != null)
            {
                list = list.Where(x => (x.Type.Trim().ToLower() == criteria.Type.Trim().ToLower() && x.ServiceTypeId.IndexOf(criteria.ServiceTypeId)>-1 && x.Inactive == criteria.Inactive)).ToList();
            }

            rowsCount = list.Count;
            if (size > 1)
            {
                if (page < 1)
                {
                    page = 1;
                }
                list = list.OrderByDescending(x => x.DatetimeModified).Skip((page - 1) * size).Take(size).ToList();
            }
            foreach(var charge in list)
            {
                var currency = ((eFMSDataContext)DataContext.DC).CatCurrency.Where(x => x.Id == charge.CurrencyId).FirstOrDefault();
                var unit = ((eFMSDataContext)DataContext.DC).CatUnit.Where(x => x.Id == charge.UnitId).FirstOrDefault();
                //var listServices = charge.ServiceTypeId.Split(";");
                var chargeDefaultAccounts = ((eFMSDataContext)DataContext.DC).CatChargeDefaultAccount.Where(x => x.ChargeId == charge.Id).ToList();
                var obj = new { currency = currency?.Id, unit = unit?.Code, charge, chargeDefaultAccounts };
                listReturn.Add(obj);
            }
            
            return listReturn;
        }

        public List<CatCharge> Query(CatChargeCriteria criteria)
        {
            var list = DataContext.Where(x => x.Inactive == criteria.Inactive || criteria.Inactive == null);
            if(criteria.All == null)
            {
                list = list.Where(x => ((x.ChargeNameEn ?? "").IndexOf(criteria.ChargeNameEn ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                && ((x.ChargeNameVn ?? "").IndexOf(criteria.ChargeNameVn ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                && ((x.Code ?? "").IndexOf(criteria.Code ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                && ((x.Type??"").IndexOf(criteria.Type??"",StringComparison.OrdinalIgnoreCase)>=0)
                && ((x.ServiceTypeId??"").IndexOf(criteria.ServiceTypeId+";"??"",StringComparison.OrdinalIgnoreCase)>=0));
            }
            else
            {
               list = list.Where(x => ((x.ChargeNameEn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
               || ((x.ChargeNameVn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
               || ((x.Code ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
               || ((x.Type ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
               || ((x.ServiceTypeId ?? "").IndexOf(criteria.All + ";" ?? "", StringComparison.OrdinalIgnoreCase) >= 0));
            }
            return list.ToList(); ;
        }

        public HandleState DeleteCharge(Guid id)
        {
            DataContext.Delete(x => x.Id == id);
            try
            {
                DataContext.Delete(x => x.Id == id);
                var listChargeDefaultAccount = ((eFMSDataContext)DataContext.DC).CatChargeDefaultAccount.Where(x => x.ChargeId == id).ToList();
                foreach(var item in listChargeDefaultAccount)
                {
                    ((eFMSDataContext)DataContext.DC).CatChargeDefaultAccount.Remove(item);
                }
                ((eFMSDataContext)DataContext.DC).SaveChanges();
                var hs = new HandleState();
                return hs;

            }
            catch(Exception ex)
            {
                var hs = new HandleState(ex.Message);
                return hs;
            }

        }

        public List<CatChargeImportModel> CheckValidImport(List<CatChargeImportModel> list)
        {
            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            var charges = dc.CatCharge.ToList();
            list.ForEach(item =>
            {
                if (string.IsNullOrEmpty(item.ChargeNameEn))
                {
                    item.ChargeNameEn = stringLocalizer[LanguageSub.MSG_CHARGE_NAME_EN_EMPTY];
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.ChargeNameVn))
                {
                    item.ChargeNameVn = stringLocalizer[LanguageSub.MSG_CHARGE_NAME_LOCAL_EMPTY];
                    item.IsValid = false;
                }
                if (item.UnitId<=0)
                {
                    item.UnitId = -1;
                    item.IsValid = false;
                }
                if (item.UnitId > 0)
                {
                    var unit = dc.CatUnit.FirstOrDefault(x => x.Id == item.UnitId);
                    if (unit == null)
                    {
                        item.UnitId = -1;
                        item.IsValid = false;
                    }
                }
                if (item.UnitPrice < 0)
                {
                    item.UnitPrice = -1;
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.CurrencyId))
                {
                    item.CurrencyId = stringLocalizer[LanguageSub.MSG_CHARGE_CURRENCY_EMPTY];
                    item.IsValid = false;
                }
                if (!string.IsNullOrEmpty(item.CurrencyId))
                {
                    var currency = dc.CatCurrency.FirstOrDefault(x => x.Id == item.CurrencyId);
                    if (currency == null)
                    {
                        item.CurrencyId = stringLocalizer[LanguageSub.MSG_CHARGE_CURRENCY_NOT_FOUND];
                        item.IsValid = false;
                    }
                }
                if (string.IsNullOrEmpty(item.Type))
                {
                    item.Type = stringLocalizer[LanguageSub.MSG_CHARGE_TYPE_EMPTY];
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.ServiceTypeId))
                {
                    item.ServiceTypeId = stringLocalizer[LanguageSub.MSG_CHARGE_SERVICE_TYPE_EMPTY];
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.Code))
                {
                    item.Code = stringLocalizer[LanguageSub.MSG_CHARGE_CODE_EMPTY];
                    item.IsValid = false;
                }
                if(!string.IsNullOrEmpty(item.Code))
                {
                    var charge = charges.FirstOrDefault(x => x.Code.ToLower() == item.Code?.ToLower());
                    if (charge != null)
                    {
                        item.Code = string.Format(stringLocalizer[LanguageSub.MSG_CHARGE_CODE_EXISTED], item.Code);
                        item.IsValid = false;
                    }
                    if (list.Count(x => x.Code?.ToLower() == item.Code?.ToLower()) > 1)
                    {
                        item.Code = string.Format(stringLocalizer[LanguageSub.MSG_CHARGE_CODE_DUPLICATED], item.Code);
                        item.IsValid = false;
                    }
                }

            });
            return list;
        }

        public HandleState Import(List<CatChargeImportModel> data)
        {
            try
            {
                eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
                foreach(var item in data)
                {
                    var charge = new CatCharge
                    {
                        Id = Guid.NewGuid(),
                        Code = item.Code,
                        ChargeNameEn = item.ChargeNameEn,
                        ChargeNameVn = item.ChargeNameVn,
                        UnitId = item.UnitId,
                        UnitPrice = item.UnitPrice,
                        CurrencyId = item.CurrencyId,
                        Type = item.Type,
                        ServiceTypeId = item.ServiceTypeId,
                        Inactive = item.Status.ToString().ToLower() == "active" ? false : true,
                        DatetimeCreated = DateTime.Now,
                        UserCreated = ChangeTrackerHelper.currentUser
                    };
                    dc.CatCharge.Add(charge);
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
