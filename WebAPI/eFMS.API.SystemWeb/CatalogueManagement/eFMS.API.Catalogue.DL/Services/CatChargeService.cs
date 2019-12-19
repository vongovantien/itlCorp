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
using ITL.NetCore.Connection.Caching;
using System.Linq.Expressions;

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatChargeService  : RepositoryBaseCache<CatCharge,CatChargeModel>, ICatChargeService
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICurrentUser currentUser;
        private readonly IContextBase<CatChargeDefaultAccount> chargeDefaultRepository;
        private readonly ICatCurrencyService currencyService;
        private readonly ICatUnitService catUnitService;

        public CatChargeService(IContextBase<CatCharge> repository, 
            ICacheServiceBase<CatCharge> cacheService, 
            IMapper mapper,
            IStringLocalizer<LanguageSub> localizer,
            ICurrentUser user,
            IContextBase<CatChargeDefaultAccount> chargeDefaultRepo,
            ICatCurrencyService currService,
            ICatUnitService unitService) : base(repository, cacheService, mapper)
        {
            stringLocalizer = localizer;
            chargeDefaultRepository = chargeDefaultRepo;
            currencyService = currService;
            currentUser = user;
            catUnitService = unitService;
            SetChildren<CsShipmentSurcharge>("Id", "ChargeId");
        }

        public HandleState AddCharge(CatChargeAddOrUpdateModel model)
        {
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                Guid chargeId = Guid.NewGuid();
                model.Charge.Id = chargeId;
                model.Charge.Active = true;
                model.Charge.UserCreated = model.Charge.UserModified = currentUser.UserID;
                model.Charge.DatetimeCreated = DateTime.Now;

                try
                {
                    var hs = DataContext.Add(model.Charge, false);
                    if (hs.Success == false) return hs;
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
                    ClearCache();
                    Get();
                    trans.Commit();
                    return hs;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    return new HandleState(ex.Message);
                }
                finally
                {
                    trans.Dispose();
                }
            }
        }

        public HandleState UpdateCharge(CatChargeAddOrUpdateModel model)
        {
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    model.Charge.UserModified = currentUser.UserID;
                    model.Charge.DatetimeModified = DateTime.Now;
                    var hs = DataContext.Update(model.Charge, x => x.Id == model.Charge.Id, false);
                    if (hs.Success == false) return hs;
                    foreach (var item in model.ListChargeDefaultAccount)
                    {
                        if (item.Id == 0)
                        {
                            item.ChargeId = model.Charge.Id;
                            item.UserCreated = item.UserModified = currentUser.UserID;
                            item.DatetimeCreated = item.DatetimeModified = DateTime.Now;
                            chargeDefaultRepository.Add(item, false);
                        }
                        else
                        {
                            item.UserModified = currentUser.UserID;
                            item.DatetimeModified = DateTime.Now;
                            chargeDefaultRepository.Update(item, x => x.Id == item.Id, false);
                        }
                    }
                    chargeDefaultRepository.SubmitChanges();
                    DataContext.SubmitChanges();
                    trans.Commit();
                    ClearCache();
                    Get();
                    return hs;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    return new HandleState(ex.Message);
                }
                finally
                {
                    trans.Dispose();
                }
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
            Expression<Func<CatChargeModel, bool>> query = null;
            if (criteria.All == null)
            {
                query = x => (x.ChargeNameEn ?? "").IndexOf(criteria.ChargeNameEn ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                    && (x.ChargeNameVn ?? "").IndexOf(criteria.ChargeNameVn ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                    && (x.Code ?? "").IndexOf(criteria.Code ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                    && (x.Type ?? "").IndexOf(criteria.Type ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                    && (x.ServiceTypeId ?? "").IndexOf(criteria.ServiceTypeId ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                    && (x.Active == criteria.Active || criteria.Active == null);
            }
            else
            {
                query = x => ((x.ChargeNameEn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                   || (x.ChargeNameVn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                   || (x.Code ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                   || (x.Type ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                   || (x.ServiceTypeId ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1)
                                   && (x.Active == criteria.Active || criteria.Active == null);
            }
            var data = Paging(query, page, size, x => x.DatetimeModified, false, out rowsCount);
            return data;
        }

        public IQueryable<CatChargeModel> Query(CatChargeCriteria criteria)
        {
            Expression<Func<CatChargeModel, bool>> query = null;
            if (criteria.All == null)
            {
                query = x => (x.ChargeNameEn ?? "").IndexOf(criteria.ChargeNameEn ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                    && (x.ChargeNameVn ?? "").IndexOf(criteria.ChargeNameVn ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                    && (x.Code ?? "").IndexOf(criteria.Code ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                    && (x.Type ?? "").IndexOf(criteria.Type ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                    && (x.ServiceTypeId ?? "").IndexOf(criteria.ServiceTypeId ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                    && (x.Active == criteria.Active || criteria.Active == null);
            }
            else
            {
                query = x => ((x.ChargeNameEn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                   || (x.ChargeNameVn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                   || (x.Code ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                   || (x.Type ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                   || (x.ServiceTypeId ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1)
                                   && (x.Active == criteria.Active || criteria.Active == null);
            }
            var list = Get(query);
            var currencies = currencyService.Get();
            var units = catUnitService.Get();
            list = list.Join(currencies, x => x.CurrencyId, y => y.Id, (x, y) => new { x, y.CurrencyName })
                        .Join(units, x => x.x.UnitId, y => y.Id, (x, y) => new CatChargeModel {
                            Id = x.x.Id,
                            Code = x.x.Code,
                            ChargeNameVn = x.x.ChargeNameVn,
                            ChargeNameEn = x.x.ChargeNameEn,
                            ServiceTypeId = x.x.ServiceTypeId,
                            Type = x.x.Type,
                            CurrencyId = x.x.CurrencyId,
                            UnitPrice = x.x.UnitPrice,
                            UnitId = x.x.UnitId,
                            Vatrate = x.x.Vatrate,
                            IncludedVat = x.x.IncludedVat,
                            UserCreated = x.x.UserCreated,
                            DatetimeCreated = x.x.DatetimeCreated,
                            UserModified = x.x.UserModified,
                            DatetimeModified = x.x.DatetimeModified,
                            Active = x.x.Active,
                            InactiveOn = x.x.InactiveOn,
                            currency = y.UnitNameEn,
                            unit = y.UnitNameEn
                        });
            return list;
        }

        public HandleState DeleteCharge(Guid id)
        {
            using (var trans = DataContext.DC.Database.BeginTransaction())
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
                            chargeDefaultRepository.Delete(x => x.Id == item.Id, false);
                        }
                        chargeDefaultRepository.SubmitChanges();
                        DataContext.SubmitChanges();
                        ClearCache();
                        Get();
                        trans.Commit();
                    }
                    return hs;

                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    return new HandleState(ex.Message);
                }
                finally
                {
                    trans.Dispose();
                }
            }
        }

        public List<CatChargeImportModel> CheckValidImport(List<CatChargeImportModel> list)
        {
            var charges = Get();
            var units = catUnitService.Get();
            var currencies = currencyService.Get();
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
                    var charge = charges.FirstOrDefault(x => x.Code.ToLower() == item.Code.ToLower());
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
                    DataContext.Add(charge, false);
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
