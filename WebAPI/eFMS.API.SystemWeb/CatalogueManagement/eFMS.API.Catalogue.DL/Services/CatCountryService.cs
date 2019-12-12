using AutoMapper;
using AutoMapper.QueryableExtensions;
using eFMS.API.Catalogue.DL.Common;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.DL.ViewModels;
using eFMS.API.Catalogue.Service.Contexts;
using eFMS.API.Catalogue.Service.Models;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Helpers;
using eFMS.API.Common.NoSql;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.Caching;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatCountryService : RepositoryBaseCache<CatCountry, CatCountryModel>, ICatCountryService
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICurrentUser currentUser;

        public CatCountryService(IContextBase<CatCountry> repository, 
            ICacheServiceBase<CatCountry> cacheService, 
            IMapper mapper,
            IStringLocalizer<LanguageSub> localizer,
            ICurrentUser user) : base(repository, cacheService, mapper)
        {
            stringLocalizer = localizer;
            currentUser = user;
            SetChildren<CatPlace>("Id", "CountryId");
            SetChildren<CatPartner>("Id", "CountryId");
            SetChildren<CatPartner>("Id", "CountryShippingId");
            SetChildren<CsTransactionDetail>("Id", "OriginCountryId");
        }

        #region CRUD
        public override HandleState Add(CatCountryModel entity)
        {
            entity.DatetimeCreated = entity.DatetimeModified = DateTime.Now;
            entity.UserCreated = entity.UserModified = currentUser.UserID;
            entity.Active = true;
            var country = mapper.Map<CatCountry>(entity);
            var result = DataContext.Add(country);
            if (result.Success)
            {
                ClearCache();
                Get();
            }
            return result;
        }

        public HandleState Delete(short id)
        {
            ChangeTrackerHelper.currentUser = currentUser.UserID;
            var hs = DataContext.Delete(x => x.Id == id);
            if (hs.Success)
            {
                ClearCache();
                Get();
            }
            return hs;
        }
        #endregion

        public List<CatCountryViewModel> GetByLanguage()
        {
            var data = Get();
            return GetDataByLanguage(data);
        }
        
        public IQueryable<CatCountryModel> GetCountries(CatCountryCriteria criteria, int page, int size, out int rowsCount)
        {
            Expression<Func<CatCountryModel, bool>> query = null;
            if (criteria.condition == SearchCondition.AND)
            {
                query = x => (x.Code ?? "").IndexOf(criteria.Code ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                        && (x.NameEn ?? "").IndexOf(criteria.NameEn ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                        && (x.NameVn ?? "").IndexOf(criteria.NameVn ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                        && (x.Active == criteria.Active || criteria.Active == null);
            }
            else
            {
                query = x => ((x.Code ?? "").IndexOf(criteria.Code ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                                                || (x.NameEn ?? "").IndexOf(criteria.NameEn ?? "null", StringComparison.OrdinalIgnoreCase) > -1
                                                                || (x.NameVn ?? "").IndexOf(criteria.NameVn ?? "null", StringComparison.OrdinalIgnoreCase) > -1)
                                                                && (x.Active == criteria.Active || criteria.Active == null);
            }
            var data = Paging(query, page, size, x => x.DatetimeModified, false, out rowsCount);
            return data;    
        }

        #region Import
        public HandleState Import(List<CatCountryImportModel> data)
        {
            try
            {
                var newList = new List<CatCountry>();
                foreach (var item in data)
                {
                    bool active = !string.IsNullOrEmpty(item.Status) && (item.Status.ToLower() == "active");
                    DateTime? inactiveDate = active == false ? (DateTime?)DateTime.Now : null;
                    var country = new CatCountry
                    {
                        Code = item.Code,
                        NameEn = item.NameEn,
                        NameVn = item.NameVn,
                        DatetimeCreated = DateTime.Now,
                        UserCreated = currentUser.UserID,
                        Active = active,
                        InactiveOn = inactiveDate
                    };
                    DataContext.Add(country, false);
                }
                DataContext.SubmitChanges();
                ClearCache();
                Get();
                return new HandleState();
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }
        
        public List<CatCountryImportModel> CheckValidImport(List<CatCountryImportModel> list)
        {
            var countries = Get();
            list.ForEach(item =>
            {
                if (string.IsNullOrEmpty(item.NameEn))
                {
                    item.NameEn = stringLocalizer[LanguageSub.MSG_COUNTRY_NAME_EN_EMPTY];
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.NameVn))
                {
                    item.NameVn = stringLocalizer[LanguageSub.MSG_COUNTRY_NAME_LOCAL_EMPTY];
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.Code))
                {
                    item.Code = stringLocalizer[LanguageSub.MSG_COUNTRY_CODE_EMPTY];
                    item.IsValid = false;
                }
                else
                {
                    var country = countries.FirstOrDefault(x => x.Code.ToLower() == item.Code.ToLower());
                    if (country != null)
                    {
                        item.Code = string.Format(stringLocalizer[LanguageSub.MSG_COUNTRY_EXISTED], item.Code);
                        item.IsValid = false;
                    }
                    if (list.Count(x => x.Code.ToLower() == item.Code.ToLower()) > 1)
                    {
                        item.Code = string.Format(stringLocalizer[LanguageSub.MSG_COUNTRY_CODE_DUPLICATE], item.Code);
                        item.IsValid = false;
                    }
                }
            });
            return list;
        }
        #endregion

        public IQueryable<CatCountryModel> Query(CatCountryCriteria criteria)
        {
            IQueryable<CatCountryModel> data = GetBy(criteria);
            return data;
        }

        private IQueryable<CatCountryModel> GetBy(CatCountryCriteria criteria)
        {
            Expression<Func<CatCountryModel, bool>> query = null;
            if (criteria.condition == SearchCondition.AND)
            {
                query = x => (x.Code ?? "").IndexOf(criteria.Code ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                        && (x.NameEn ?? "").IndexOf(criteria.NameEn ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                        && (x.NameVn ?? "").IndexOf(criteria.NameVn ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                        && (x.Active == criteria.Active || criteria.Active == null);
            }
            else
            {
                query = x => ((x.Code ?? "").IndexOf(criteria.Code ?? "", StringComparison.OrdinalIgnoreCase) > -1
                            || (x.NameEn ?? "").IndexOf(criteria.NameEn ?? "", StringComparison.OrdinalIgnoreCase) > -1
                            || (x.NameVn ?? "").IndexOf(criteria.NameVn ?? "", StringComparison.OrdinalIgnoreCase) > -1)
                            && (x.Active == criteria.Active || criteria.Active == null);
            }
            var data = Get(query);
            return data;
        }

        private List<CatCountryViewModel> GetDataByLanguage(IQueryable<CatCountry> data)
        {
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            var results = new List<CatCountryViewModel>();
            if (currentCulture.Name == "vi-VN")
            {
                foreach (var item in data)
                {
                    var country = new CatCountryViewModel
                    {
                        Id = item.Id,
                        Code = item.Code,
                        Name = item.NameVn,
                        UserCreated = item.UserCreated,
                        DatetimeCreated = item.DatetimeCreated,
                        UserModified = item.UserModified,
                        DatetimeModified = item.DatetimeModified,
                        Active = item.Active,
                        InActiveOn = item.InactiveOn
                    };
                    results.Add(country);
                }
            }
            else
            {
                foreach (var item in data)
                {
                    var country = new CatCountryViewModel
                    {
                        Id = item.Id,
                        Code = item.Code,
                        Name = item.NameEn,
                        UserCreated = item.UserCreated,
                        DatetimeCreated = item.DatetimeCreated,
                        UserModified = item.UserModified,
                        DatetimeModified = item.DatetimeModified,
                        Active = item.Active,
                        InActiveOn = item.InactiveOn
                    };
                    results.Add(country);
                }
            }
            return results;
        }

        public HandleState Update(CatCountryModel model)
        {

            model.DatetimeModified = DateTime.Now;
            model.UserModified = currentUser.UserID;
            if (model.Active == false)
            {
                model.InactiveOn = DateTime.Now;
            }
            var result = Update(model, x => x.Id == model.Id);
            if (result.Success)
            {
                ClearCache();
                Get();
            }
            return result;
        }
    }
}
