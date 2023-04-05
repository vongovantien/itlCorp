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
    public class CatDistrictService : RepositoryBaseCache<CatDistrict, CatDistrictModel>, ICatDistrictService
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICurrentUser currentUser;
        private readonly IContextBase<CatCity> catCityRepository;
        private readonly IContextBase<CatCountry> catCountryRepository;


        public CatDistrictService(IContextBase<CatDistrict> repository,
           ICacheServiceBase<CatDistrict> cacheService,
           IMapper mapper,
           IStringLocalizer<LanguageSub> localizer,
           IContextBase<CatCity> catCityRepo,
           IContextBase<CatCountry> catCountryRepo,

           ICurrentUser user) : base(repository, cacheService, mapper)
        {
            stringLocalizer = localizer;
            currentUser = user;
            catCityRepository = catCityRepo;
            catCountryRepository = catCountryRepo;
            SetChildren<CatPlace>("Id", "CountryId");
            SetChildren<CatPartner>("Id", "CountryId");

        }

        #region CRUD
        public override HandleState Add(CatDistrictModel entity)
        {
            entity.DatetimeCreated = entity.DatetimeModified = DateTime.Now;
            entity.UserCreated = entity.UserModified = currentUser.UserID;
            entity.Active = true;
            var District = mapper.Map<CatDistrict>(entity);
            var result = DataContext.Add(District);
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

        public List<CatDistrictViewModel> GetByLanguage()
        {
            var data = Get();
            if (data == null) return null;
            return GetDataByLanguage(data);
        }

        public IQueryable<CatDistrictModel> GetDistricts(CatDistrictCriteria criteria, int page, int size, out int rowsCount)
        {
            Expression<Func<CatDistrictModel, bool>> query = null;
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
            return data;
        }

        public HandleState Import(List<CatDistrictModel> data)
        {
            try
            {
                var newList = new List<CatDistrict>();
                foreach (var item in data)
                {
                    bool active = string.IsNullOrEmpty(item.Status) || (item.Status.ToLower() == "active");
                    var city = catCityRepository.Where(x => x.Code == item.CodeCountry)?.FirstOrDefault();
                    var country = catCityRepository.Where(x => x.Code == item.CodeCountry)?.FirstOrDefault();

                    DateTime? inactiveDate = active == false ? (DateTime?)DateTime.Now : null;
                    var district = new CatDistrict
                    {
                        Code = item.Code,
                        NameEn = item.NameEn,
                        NameVn = item.NameVn,
                        CodeCity = city?.Code,
                        CodeCountry = country?.Code,
                        DatetimeCreated = DateTime.Now,
                        UserCreated = currentUser.UserID,
                        UserModified = currentUser.UserID,
                        DatetimeModified = DateTime.Now,
                        Active = active,
                        InactiveOn = inactiveDate
                    };
                    newList.Add(district);
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

        public IQueryable<CatDistrictModel> Query(CatDistrictCriteria criteria)
        {
            Expression<Func<CatDistrictModel, bool>> query = null;
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

        public HandleState Update(CatDistrictModel model)
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

        private List<CatDistrictViewModel> GetDataByLanguage(IQueryable<CatDistrict> data)
        {
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            var results = new List<CatDistrictViewModel>();
            foreach (var item in data)
            {
                var city = new CatDistrictViewModel
                {
                    Id = item.Id,
                    Code = item.Code,
                    Name = currentCulture.IetfLanguageTag == "en-US" ? item.NameEn : item.NameVn,
                    UserCreated = item.UserCreated,
                    DatetimeCreated = item.DatetimeCreated,
                    UserModified = item.UserModified,
                    DatetimeModified = item.DatetimeModified,
                    Active = item.Active,
                    InActiveOn = item.InactiveOn,
                    CodeCity = item.CodeCity,
                    CodeCountry = item.CodeCountry
                };
                results.Add(city);
            }
            return results;
        }

        public List<CatDistrictModel> CheckValidImport(List<CatDistrictModel> list)
        {
            var cities = Get();
            list.ForEach(item =>
            {
                if (string.IsNullOrEmpty(item.NameEn))
                {
                    item.NameEn = stringLocalizer[CatalogueLanguageSub.MSG_DISTRICT_NAME_EN_EMPTY];
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.NameVn))
                {
                    item.NameVn = stringLocalizer[CatalogueLanguageSub.MSG_DISTRICT_NAME_LOCAL_EMPTY];
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.Code))
                {
                    item.Code = stringLocalizer[CatalogueLanguageSub.MSG_DISTRICT_CODE_EMPTY];
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.CodeCity))
                {
                    item.CodeCity = stringLocalizer[CatalogueLanguageSub.MSG_CITY_CODE_EMPTY];
                    item.IsValid = false;
                }
                else
                {
                    var city = cities.FirstOrDefault(x => x.Code.ToLower() == item.Code.ToLower() && x.CodeCity.ToUpper() == item.CodeCity.ToUpper());
                    if (city != null)
                    {
                        item.Code = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_DISTRICT_EXISTED], item.Code);
                        item.IsValid = false;
                    }
                    if (list.Count(x => x.Code.ToLower() == item.Code.ToLower() && x.CodeCity.ToUpper() == item.CodeCity.ToUpper()) > 1)
                    {
                        item.Code = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_DISTRICT_CODE_DUPLICATE], item.Code);
                        item.IsValid = false;
                    }
                }
            });
            return list;
        }
        public List<CatDistrict> GetDistrictsByCity(string CityCode)
        {
            var data = DataContext.Get();
            return data.Where(x => x.CodeCity == CityCode).ToList();
        }
    }
}
