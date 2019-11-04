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
    public class CatCountryService : RepositoryBase<CatCountry, CatCountryModel>, ICatCountryService
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly IDistributedCache cache;
        private readonly ICurrentUser currentUser;
        public CatCountryService(IContextBase<CatCountry> repository, IMapper mapper, IStringLocalizer<LanguageSub> localizer, IDistributedCache distributedCache, ICurrentUser user) : base(repository, mapper)
        {
            stringLocalizer = localizer;
            cache = distributedCache;
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
            var hs = DataContext.Add(country);
            if (hs.Success)
            {
                cache.Remove(Templates.CatCountry.NameCaching.ListName);
                RedisCacheHelper.SetObject(cache, Templates.CatCountry.NameCaching.ListName, DataContext.Get());
            }
            return hs;
        }
        public HandleState Update(CatCountryModel model)
        {
            var entity = mapper.Map<CatCountry>(model);
            entity.DatetimeModified = DateTime.Now;
            entity.UserModified = currentUser.UserID;
            if (entity.Active == false)
            {
                entity.InactiveOn = DateTime.Now;
            }
            var hs = DataContext.Update(entity, x => x.Id == model.Id);
            if (hs.Success)
            {
                cache.Remove(Templates.CatCountry.NameCaching.ListName);
                RedisCacheHelper.SetObject(cache, Templates.CatCountry.NameCaching.ListName, DataContext.Get());
            }
            return hs;
        }
        public HandleState Delete(short id)
        {
            ChangeTrackerHelper.currentUser = currentUser.UserID;
            var hs = DataContext.Delete(x => x.Id == id);
            if (hs.Success)
            {
                cache.Remove(Templates.CatCountry.NameCaching.ListName);
                RedisCacheHelper.SetObject(cache, Templates.CatCountry.NameCaching.ListName, DataContext.Get());
            }
            return hs;
        }
        #endregion

        public List<CatCountryViewModel> GetByLanguage()
        {
            IQueryable<CatCountry> data = RedisCacheHelper.Get<CatCountry>(cache, Templates.CatCountry.NameCaching.ListName);
            if (data == null)
            {
                data = DataContext.Get();
                RedisCacheHelper.SetObject(cache, Templates.CatCountry.NameCaching.ListName, data);
            }
            return GetDataByLanguage(data);
        }
        
        public IQueryable<CatCountryModel> GetCountries(CatCountryCriteria criteria, int page, int size, out int rowsCount)
        {
            IQueryable<CatCountryModel> returnList = null;
            var data = GetBy(criteria);
            rowsCount = data.Count();
            if (rowsCount == 0)
                return returnList;
            else data = data.OrderByDescending(x => x.DatetimeModified);
            if (size > 1)
            {
                if (page < 1)
                {
                    page = 1;
                }
                returnList = data.Skip((page - 1) * size).Take(size).ProjectTo<CatCountryModel>(mapper.ConfigurationProvider);
            }
            return returnList;        
        }

        #region Import
        public HandleState Import(List<CatCountryImportModel> data)
        {
            try
            {
                var newList = new List<CatCountry>();
                foreach (var item in data)
                {
                    DateTime? Active = null;
                    bool isActive = item.Status.Contains("active");
                    var country = new CatCountry
                    {
                        Code = item.Code,
                        NameEn = item.NameEn,
                        NameVn = item.NameVn,
                        DatetimeCreated = DateTime.Now,
                        UserCreated = currentUser.UserID,
                        Active = isActive, // item.Status == "active",// (item.Status ?? "").Contains("active"),
                        InactiveOn = isActive == false? DateTime.Now: Active
                    };
                    DataContext.Add(country, false);
                }
                //dc.SaveChanges();
                DataContext.SubmitChanges();
                cache.Remove(Templates.CatCountry.NameCaching.ListName);

                return new HandleState();
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }
        
        public List<CatCountryImportModel> CheckValidImport(List<CatCountryImportModel> list)
        {
            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            var countries = dc.CatCountry.ToList();
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

        public IQueryable<CatCountry> Query(CatCountryCriteria criteria)
        {
            IQueryable<CatCountry> data = GetBy(criteria);
            if (data == null) return null;
            return data.ProjectTo<CatCountryModel>(mapper.ConfigurationProvider);
        }

        private IQueryable<CatCountry> GetBy(CatCountryCriteria criteria)
        {
            Expression<Func<CatCountry, bool>> query = null;
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
            IQueryable<CatCountry> data = RedisCacheHelper.Get<CatCountry>(cache, Templates.CatCountry.NameCaching.ListName);
            if (data == null)
            {
                data = DataContext.Get(query);
            }
            else
            {
                data = data.Where(query);
            }
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
    }
}
