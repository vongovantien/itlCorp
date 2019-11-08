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
using eFMS.API.Catalogue.Service.Contexts;
using eFMS.API.Common.NoSql;
using eFMS.IdentityServer.DL.UserManager;
using AutoMapper.QueryableExtensions;

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatChargeService  :RepositoryBase<CatCharge,CatChargeModel>,ICatChargeService
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICurrentUser currentUser;
        private readonly IContextBase<CatChargeDefaultAccount> chargeDefaultRepository;
        private readonly IContextBase<CatCurrency> currencyRepository;
        private readonly IContextBase<CatUnit> unitRepository;
        public CatChargeService(IContextBase<CatCharge> repository,
            IMapper mapper, 
            IStringLocalizer<LanguageSub> localizer, 
            ICurrentUser user,
            IContextBase<CatChargeDefaultAccount> chargeDefaultRepo,
            IContextBase<CatCurrency> currencyRepo,
            IContextBase<CatUnit> unitRepo
            ) :base(repository,mapper)
        {
            stringLocalizer = localizer;
            chargeDefaultRepository = chargeDefaultRepo;
            currencyRepository = currencyRepo;
            currentUser = user;
            unitRepository = unitRepo;
            SetChildren<CsShipmentSurcharge>("Id", "ChargeId");
        }

        public HandleState AddCharge(CatChargeAddOrUpdateModel model)
        {
            Guid chargeId = Guid.NewGuid();
            model.Charge.Id = chargeId;
            model.Charge.Active = true;
            model.Charge.UserCreated = model.Charge.UserModified = currentUser.UserID;
            model.Charge.DatetimeCreated = DateTime.Now;

            try
            {
                DataContext.Add(model.Charge, false);

                foreach (var x in model.ListChargeDefaultAccount)
                {
                    x.ChargeId = chargeId;
                    x.Active = true;
                    x.UserCreated = x.UserModified = currentUser.UserID;
                    x.DatetimeCreated = DateTime.Now;
                    chargeDefaultRepository.Add(x, false);
                }
                chargeDefaultRepository.SubmitChanges();
                DataContext.SubmitChanges();
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
            model.Charge.UserModified = currentUser.UserID;
            model.Charge.DatetimeModified = DateTime.Now;
            try
            {
                DataContext.Update(model.Charge, x => x.Id == model.Charge.Id, false);
                foreach(var item in model.ListChargeDefaultAccount)
                {
                    item.UserModified = currentUser.UserID;
                    item.DatetimeModified = DateTime.Now;
                    chargeDefaultRepository.Update(item, x => x.Id == item.Id, false);
                }
                chargeDefaultRepository.SubmitChanges();
                DataContext.SubmitChanges();
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
            var listChargeDefault = chargeDefaultRepository.Get(x => x.ChargeId == id).ToList();
            returnCharge.Charge = charge;
            returnCharge.ListChargeDefaultAccount = listChargeDefault;
            return returnCharge;
        }



        public IQueryable<CatChargeModel> Paging(CatChargeCriteria criteria, int page, int size, out int rowsCount)
        {
            var list = Query(criteria);
            list = list.OrderByDescending(x => x.DatetimeModified);
            rowsCount = list.Count();
            if (rowsCount == 0) return null;
            if (size > 1)
            {
                if (page < 1)
                {
                    page = 1;
                }
                list = list.Skip((page - 1) * size).Take(size);
            }
            return list.ProjectTo<CatChargeModel>(mapper.ConfigurationProvider);
        }

        public IQueryable<CatCharge> Query(CatChargeCriteria criteria)
        {
            var list = DataContext.Where(x => x.Active == criteria.Active || criteria.Active == null);
            var currencies = currencyRepository.Get();
            var units = unitRepository.Get();
            if(criteria.All == null)
            {
                list = list.Where(x => ((x.ChargeNameEn ?? "").IndexOf(criteria.ChargeNameEn ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                && ((x.ChargeNameVn ?? "").IndexOf(criteria.ChargeNameVn ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                && ((x.Code ?? "").IndexOf(criteria.Code ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                && ((x.Type ?? "").IndexOf(criteria.Type ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                && ((x.ServiceTypeId ?? "").IndexOf(criteria.ServiceTypeId ?? "", StringComparison.OrdinalIgnoreCase) >= 0));
            }
            else
            {
               list = list.Where(x => ((x.ChargeNameEn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
               || ((x.ChargeNameVn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
               || ((x.Code ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
               || ((x.Type ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
               || ((x.ServiceTypeId ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0));
            }
            return list;
        }

        public HandleState DeleteCharge(Guid id)
        {
            try
            {
                ChangeTrackerHelper.currentUser = currentUser.UserID;
                var hs = DataContext.Delete(x => x.Id == id, false);
                if (hs.Success)
                {
                    var listChargeDefaultAccount = chargeDefaultRepository.Get(x => x.ChargeId == id).ToList();
                    foreach (var item in listChargeDefaultAccount)
                    {
                        //((eFMSDataContext)DataContext.DC).CatChargeDefaultAccount.Remove(item);
                        chargeDefaultRepository.Delete(x => x.Id == item.Id, false);
                    }
                }
                chargeDefaultRepository.SubmitChanges();
                DataContext.SubmitChanges();
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
            //eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            var charges = DataContext.Get().ToList();
            var units = unitRepository.Get().ToList();
            var currencies = currencyRepository.Get().ToList();
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
                //if (item.UnitId<=0)
                //{
                //    item.UnitId = -1;
                //    item.IsValid = false;
                //}
                //if (item.UnitId > 0)
                //{
                //    var unit = dc.CatUnit.FirstOrDefault(x => x.Id == item.UnitId);
                //    if (unit == null)
                //    {
                //        item.UnitId = -1;
                //        item.IsValid = false;
                //    }
                //}
                if (string.IsNullOrEmpty(item.UnitCode))
                {
                    item.UnitCode = stringLocalizer[LanguageSub.MSG_CHARGE_UNIT_EMPTY];
                    item.IsValid = false;
                }
                else
                {
                    var unit = units.FirstOrDefault(x => x.Code.ToLower() == item.UnitCode.ToLower());
                    if(unit == null)
                    {
                        item.UnitCode = stringLocalizer[LanguageSub.MSG_CHARGE_UNIT_NOT_FOUND];
                        item.IsValid = false;
                    }
                    else
                    {
                        item.UnitId = unit.Id;
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
                    var currency = currencies.FirstOrDefault(x => x.Id == item.CurrencyId);
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
                // eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
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
                        Active = item.Status.Trim().ToLower() == "active",
                        DatetimeCreated = DateTime.Now,
                        UserCreated = currentUser.UserID,
                        UserModified = currentUser.UserID
                    };
                    //dc.CatCharge.Add(charge);
                    DataContext.Add(charge, false);
                }
                DataContext.SubmitChanges();
                return new HandleState();
            }
            catch(Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }

        public IQueryable<CatChargeModel> GetSettlePaymentCharges(string keySearch, bool? active,int? size)
        {
            IQueryable<CatChargeModel> list = null;
            if(size != null)
            {
                int pageSize = (int)size;
                list = Paging(x => x.Type != "DEBIT" && (x.Active == active || active == null)
                                                     && (x.Code.IndexOf(keySearch ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                                            || x.ChargeNameEn.IndexOf(keySearch ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                                            || x.ChargeNameVn.IndexOf(keySearch ?? "", StringComparison.OrdinalIgnoreCase) > -1)
                                                            , 0, pageSize);
            }
            else
            {
                list = Get(x => x.Type != "DEBIT" && (x.Active == active || active == null)
                                                  && (x.Code.IndexOf(keySearch ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                                            || x.ChargeNameEn.IndexOf(keySearch ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                                            || x.ChargeNameVn.IndexOf(keySearch ?? "", StringComparison.OrdinalIgnoreCase) > -1)
                                                            );
            }

            return list;
        }

        public object GetListService()
        {
            return API.Common.Globals.CustomData.Services;
        }

        public IQueryable<CatChargeModel> GetBy(string type)
        {
            var data = DataContext.Get(x => x.Type == type);
            if (data == null) return null;
            return data.ProjectTo<CatChargeModel>(mapper.ConfigurationProvider);
        }
    }
}
