using AutoMapper;
using eFMS.API.Catalogue.DL.Common;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.DL.ViewModels;
using eFMS.API.Catalogue.Service.Models;
using eFMS.API.Common.Globals;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.Caching;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatCityService : RepositoryBaseCache<CatCity, CatCityModel>, ICatCityService
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICurrentUser currentUser;
        private readonly IContextBase<CatCountry> catCountryRepository;

        public CatCityService(IContextBase<CatCity> repository,
           ICacheServiceBase<CatCity> cacheService,
           IMapper mapper,
           IStringLocalizer<LanguageSub> localizer,
           IContextBase<CatCountry> catCountryRepo,
           ICurrentUser user) : base(repository, cacheService, mapper)
        {
            stringLocalizer = localizer;
            currentUser = user;
            catCountryRepository = catCountryRepo;
            SetChildren<CatPlace>("Id", "CountryId");
            SetChildren<CatPartner>("Id", "CountryId");

        }

        #region CRUD
        public override HandleState Add(CatCityModel entity)
        {
            var city = mapper.Map<CatCity>(entity);
            city.Id = Guid.NewGuid();
            city.DatetimeCreated = city.DatetimeModified = DateTime.Now;
            city.Active = true;
            city.UserCreated = city.UserModified = currentUser.UserID;
            var result = DataContext.Add(city, false);
            DataContext.SubmitChanges();
            if (result.Success)
            {
                ClearCache();
                Get();
            }
            return result;
        }

        public HandleState Delete(Guid id)
        {
            //ChangeTrackerHelper.currentUser = currentUser.UserID;
            var hs = DataContext.Delete(x => x.Id == id);
            if (hs.Success)
            {
                ClearCache();
                Get();
            }
            return hs;
        }
        #endregion

        public List<CatCityViewModel> GetByLanguage()
        {
            var data = Get();
            if (data == null) return null;
            return GetDataByLanguage(data);
        }

        public IQueryable<CatCityModel> GetCities(CatCityCriteria criteria, int page, int size, out int rowsCount)
        {
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            Expression<Func<CatCityModel, bool>> query = null;
            if (criteria.All == null)
            {
                query = x => (x.Code ?? "").IndexOf(criteria.Code ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                        && (x.NameEn ?? "").IndexOf(criteria.NameEn ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                        && (x.NameVn ?? "").IndexOf(criteria.NameVn ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                        && (x.Active == criteria.Active || criteria.Active == null);
            }
            else
            {
                query = x => ((x.Code ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                                                || (x.NameEn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                                                || (x.NameVn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1)
                                                                && (x.Active == criteria.Active || criteria.Active == null);
            }
            var data = Paging(query, page, size, x => x.DatetimeModified, false, out rowsCount);
            List<CatCityModel> lst = new List<CatCityModel>();
            lst = data.ToList();
            lst.ForEach(x => {
                var country = catCountryRepository.Where(y => y.Id == x.CountryId)?.FirstOrDefault();
                x.CountryName = currentCulture.IetfLanguageTag == "en-US" ? country?.NameEn : country?.NameVn;
            });
            return lst.AsQueryable();
        }

        public HandleState Import(List<CatCityModel> data)
        {
            try
            {
                var newList = new List<CatCity>();
                foreach (var item in data)
                {
                    bool active = string.IsNullOrEmpty(item.Status) || (item.Status.ToLower() == "active");
                    var country = catCountryRepository.Where(x => x.Code == item.CodeCountry).FirstOrDefault();
                    DateTime? inactiveDate = active == false ? (DateTime?)DateTime.Now : null;
                    var city = new CatCity
                    {
                        Id = Guid.NewGuid(),
                        Code = item.Code,
                        NameEn = item.NameEn,
                        NameVn = item.NameVn,
                        DatetimeCreated = DateTime.Now,
                        UserCreated = currentUser.UserID,
                        UserModified = currentUser.UserID,
                        DatetimeModified = DateTime.Now,
                        Active = active,
                        InactiveOn = inactiveDate,
                        CountryId = country?.Id, 
                        PostalCode = item.PostalCode
                    };
                    newList.Add(city);
                }
                var hs = DataContext.Add(newList);
                DataContext.SubmitChanges();
                if (hs.Success)
                {
                    ClearCache();
                    Get();
                }
                return hs;
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }

        public IQueryable<CatCityModel> Query(CatCityCriteria criteria)
        {
            Expression<Func<CatCityModel, bool>> query = null;
            if (criteria.All == null)
            {
                query = x => (x.Code ?? "").IndexOf(criteria.Code ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                        && (x.NameEn ?? "").IndexOf(criteria.NameEn ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                        && (x.NameVn ?? "").IndexOf(criteria.NameVn ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                        && (x.Active == criteria.Active || criteria.Active == null);
            }
            else
            {
                query = x => ((x.Code ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                            || (x.NameEn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                            || (x.NameVn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1)
                            && (x.Active == criteria.Active || criteria.Active == null);
            }
            var data = Get(query);
            return data;
        }

        public HandleState Update(CatCityModel model)
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

        private List<CatCityViewModel> GetDataByLanguage(IQueryable<CatCity> data)
        {
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            var results = new List<CatCityViewModel>();
            foreach (var item in data)
            {
                var city = new CatCityViewModel
                {
                    Id = item.Id,
                    Code = item.Code,
                    Name = currentCulture.IetfLanguageTag == "en-US" ? item.NameEn : item.NameVn,
                    UserCreated = item.UserCreated,
                    DatetimeCreated = item.DatetimeCreated,
                    UserModified = item.UserModified,
                    DatetimeModified = item.DatetimeModified,
                    Active = item.Active,
                    InActiveOn = item.InactiveOn
                };
                results.Add(city);
            }
            return results;
        }

        public List<CatCityModel> CheckValidImport(List<CatCityModel> list)
        {
            var cities = Get();
            list.ForEach(item =>
            {
                if (string.IsNullOrEmpty(item.NameEn))
                {
                    item.NameEnError = stringLocalizer[CatalogueLanguageSub.MSG_CITY_NAME_EN_EMPTY];
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.NameVn))
                {
                    item.NameVnError = stringLocalizer[CatalogueLanguageSub.MSG_CITY_NAME_LOCAL_EMPTY];
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.Code))
                {
                    item.CodeError = stringLocalizer[CatalogueLanguageSub.MSG_CITY_CODE_EMPTY];
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.CodeCountry))
                {
                    item.CodeCountryError = stringLocalizer[CatalogueLanguageSub.MSG_COUNTRY_CODE_EMPTY];
                    item.IsValid = false;
                }
                else
                {
                    var city = cities?.FirstOrDefault(x => x.Code.ToLower() == item.Code.ToLower());
                    item.CountryName = catCountryRepository.Where(x => x.Code.ToLower() == item.CodeCountry.ToLower())?.FirstOrDefault()?.NameEn;
                    if (city != null)
                    {
                        item.CodeError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_CITY_EXISTED], item.Code);
                        item.IsValid = false;
                    }
                    if (list.Count(x => x.Code.ToLower() == item.Code.ToLower() &&  x.CodeCountry.ToUpper() == item.CodeCountry.ToUpper()) > 1)
                    {
                        item.CodeError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_CITY_CODE_DUPLICATE], item.Code);
                        item.IsValid = false;
                    }
                }
            });
            return list;
        }
        public List<CatCity> GetCitiesByCountry(short? countryId)
        {
            var data = DataContext.Get();
            return data.Where(x => x.CountryId == countryId || countryId == null).ToList();
        }
    }
}   
