using AutoMapper;
using eFMS.API.Catalogue.DL.Common;
using eFMS.API.Catalogue.DL.IService;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
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
            SetChildren<CatPartner>("Id", "CountryShippingId");

        }

        #region CRUD
        public override HandleState Add(CatCityModel entity)
        {
            entity.DatetimeCreated = entity.DatetimeModified = DateTime.Now;
            entity.UserCreated = entity.UserModified = currentUser.UserID;
            entity.Active = true;
            var city = mapper.Map<CatCity>(entity);
            var result = DataContext.Add(city);
            if (result.Success)
            {
                ClearCache();
                Get();
            }
            return result;
        }

        public HandleState Delete(short id)
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

        public List<CatCityModel> GetByLanguage()
        {
            var data = Get();
            if (data == null) return null;
            return GetDataByLanguage(data);
        }

        public IQueryable<CatCityModel> GetCities(CatCityCriteria criteria, int page, int size, out int rowsCount)
        {
            throw new NotImplementedException();
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
                        Code = item.Code,
                        NameEn = item.NameEn,
                        NameVn = item.NameVn,
                        CodeCountry = country.Code,
                        DatetimeCreated = DateTime.Now,
                        UserCreated = currentUser.UserID,
                        UserModified = currentUser.UserID,
                        DatetimeModified = DateTime.Now,
                        Active = active,
                        InactiveOn = inactiveDate
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
            throw new NotImplementedException();
        }

        public HandleState Update(CatCityModel model)
        {
            throw new NotImplementedException();
        }

        private List<CatCityModel> GetDataByLanguage(IQueryable<CatCity> data)
        {
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            var results = new List<CatCityModel>();
            foreach (var item in data)
            {
                var country = new CatCityModel
                {
                    Id = item.Id,
                    Code = item.Code,
                    Name = currentCulture.IetfLanguageTag == "en-US" ? item.NameEn : item.NameVn,
                    UserCreated = item.UserCreated,
                    DatetimeCreated = item.DatetimeCreated,
                    UserModified = item.UserModified,
                    DatetimeModified = item.DatetimeModified,
                    Active = item.Active,
                    InactiveOn = item.InactiveOn
                };
                results.Add(country);
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
                    item.NameEn = stringLocalizer[CatalogueLanguageSub.MSG_CITY_NAME_EN_EMPTY];
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.NameVn))
                {
                    item.NameVn = stringLocalizer[CatalogueLanguageSub.MSG_CITY_NAME_LOCAL_EMPTY];
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.Code))
                {
                    item.Code = stringLocalizer[CatalogueLanguageSub.MSG_CITY_CODE_EMPTY];
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.CodeCountry))
                {
                    item.CodeCountry = stringLocalizer[CatalogueLanguageSub.MSG_COUNTRY_CODE_EMPTY];
                    item.IsValid = false;
                }
                else
                {
                    var city = cities.FirstOrDefault(x => x.Code.ToLower() == item.Code.ToLower() && x.CodeCountry.ToUpper() == item.CodeCountry.ToUpper());
                    if (city != null)
                    {
                        item.Code = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_CITY_EXISTED], item.Code);
                        item.IsValid = false;
                    }
                    if (list.Count(x => x.Code.ToLower() == item.Code.ToLower() &&  x.CodeCountry.ToUpper() == item.CodeCountry.ToUpper()) > 1)
                    {
                        item.Code = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_CITY_CODE_DUPLICATE], item.Code);
                        item.IsValid = false;
                    }
                }
            });
            return list;
        }
        public List<CatCity> GetCitiesByCountry(string CountryCode)
        {
            var data = DataContext.Get();
            return data.Where(x => x.CodeCountry == CountryCode).ToList();
        }
    }
}   
