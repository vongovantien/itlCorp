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
    public class CatWardService : RepositoryBaseCache<CatWard, CatWardModel>, ICatWardService
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICurrentUser currentUser;
        private readonly IContextBase<CatCity> catCityRepository;
        private readonly IContextBase<CatCountry> catCountryRepository;
        private readonly IContextBase<CatDistrict> catDistrictRepository;



        public CatWardService(IContextBase<CatWard> repository,
           ICacheServiceBase<CatWard> cacheService,
           IMapper mapper,
           IStringLocalizer<LanguageSub> localizer,
           IContextBase<CatCity> catCityRepo,
           IContextBase<CatCountry> catCountryRepo,
           IContextBase<CatDistrict> catDistrictRepo,


           ICurrentUser user) : base(repository, cacheService, mapper)
        {
            stringLocalizer = localizer;
            currentUser = user;
            catCityRepository = catCityRepo;
            catCountryRepository = catCountryRepo;
            catDistrictRepository = catDistrictRepo;
            SetChildren<CatPlace>("Id", "CountryId");
            SetChildren<CatPartner>("Id", "CountryId");

        }

        #region CRUD
        public override HandleState Add(CatWardModel entity)
        {
            var ward = mapper.Map<CatWard>(entity);
            ward.Id = Guid.NewGuid();
            ward.DatetimeCreated = ward.DatetimeModified = DateTime.Now;
            ward.Active = true;
            ward.UserCreated = ward.UserModified = currentUser.UserID;
            var result = DataContext.Add(ward, false);
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

        public List<CatWardViewModel> GetByLanguage()
        {
            var data = Get();
            if (data == null) return null;
            return GetDataByLanguage(data);
        }

        public IQueryable<CatWardModel> GetWards(CatWardCriteria criteria, int page, int size, out int rowsCount)
        {
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            Expression<Func<CatWardModel, bool>> query = null;
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
            List<CatWardModel> lst = new List<CatWardModel>();
            lst = data.ToList();

            lst.ForEach(x => {
                var city = catCityRepository.Where(y => y.Id == x.CityId)?.FirstOrDefault();
                var country = catCountryRepository.Where(y => y.Id == x.CountryId)?.FirstOrDefault();
                var district = catDistrictRepository.Where(y => y.Id == x.DistrictId)?.FirstOrDefault();

                x.DistrictName = currentCulture.IetfLanguageTag == "en-US" ? district?.NameEn : district?.NameVn;
                x.ProvinceName = currentCulture.IetfLanguageTag == "en-US" ? city?.NameEn : city?.NameVn;
                x.CountryName = currentCulture.IetfLanguageTag == "en-US" ? country?.NameEn : country?.NameVn;

            });
            return lst.AsQueryable();
        }

        public HandleState Import(List<CatWardModel> data)
        {
            try
            {
                var newList = new List<CatWard>();
                foreach (var item in data)
                {
                    bool active = string.IsNullOrEmpty(item.Status) || (item.Status.ToLower() == "active");
                    var city = catCityRepository.Where(x => x.Code == item.CodeCity)?.FirstOrDefault();
                    var country = catCountryRepository.Where(x => x.Code == item.CodeCountry)?.FirstOrDefault();
                    var district = catDistrictRepository.Where(x => x.Code == item.CodeDistrict)?.FirstOrDefault();


                    DateTime? inactiveDate = active == false ? (DateTime?)DateTime.Now : null;
                    var ward = new CatWard
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
                        CityId = city?.Id,
                        DistrictId = district?.Id
                    };
                    newList.Add(ward);
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

        public IQueryable<CatWardModel> Query(CatWardCriteria criteria)
        {
            Expression<Func<CatWardModel, bool>> query = null;
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

        public HandleState Update(CatWardModel model)
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

        private List<CatWardViewModel> GetDataByLanguage(IQueryable<CatWard> data)
        {
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            var results = new List<CatWardViewModel>();
            foreach (var item in data)
            {
                var city = new CatWardViewModel
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

        public List<CatWardModel> CheckValidImport(List<CatWardModel> list)
        {
            var wards = Get();
            list.ForEach(item =>
            {
                if (string.IsNullOrEmpty(item.NameEn))
                {
                    item.NameEnError = stringLocalizer[CatalogueLanguageSub.MSG_WARD_NAME_EN_EMPTY];
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.NameVn))
                {
                    item.NameVnError = stringLocalizer[CatalogueLanguageSub.MSG_WARD_NAME_LOCAL_EMPTY];
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.Code))
                {
                    item.CodeError = stringLocalizer[CatalogueLanguageSub.MSG_WARD_CODE_EMPTY];
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.CodeDistrict))
                {
                    item.DistrictNameError = stringLocalizer[CatalogueLanguageSub.MSG_DISTRICT_CODE_EMPTY];
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.CodeCity))
                {
                    item.ProvinceNameError = stringLocalizer[CatalogueLanguageSub.MSG_CITY_CODE_EMPTY];
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.CodeCountry))
                {
                    item.CodeCountryError = stringLocalizer[CatalogueLanguageSub.MSG_COUNTRY_CODE_EMPTY];
                    item.IsValid = false;
                }
                else
                {
                    var ward = wards.FirstOrDefault(x => x.Code.ToLower() == item.Code.ToLower());
                    item.CountryName = catCountryRepository.Where(x => x.Code.ToLower() == item.CodeCountry.ToLower())?.FirstOrDefault()?.NameEn;
                    item.ProvinceName = catCityRepository.Where(x => x.Code.ToLower() == item.CodeCity.ToLower())?.FirstOrDefault()?.NameEn;
                    item.DistrictName = catDistrictRepository.Where(x => x.Code.ToLower() == item.CodeDistrict.ToLower())?.FirstOrDefault()?.NameEn;
                    if (ward != null)
                    {
                        item.Code = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_WARD_EXISTED], item.Code);
                        item.IsValid = false;
                    }
                    if (list.Count(x => x.Code.ToLower() == item.Code.ToLower()) > 1)
                    {
                        item.Code = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_WARD_CODE_DUPLICATE], item.Code);
                        item.IsValid = false;
                    }
                }
            });
            return list;
        }
        public List<CatWard> GetWardsByDistrict(Guid districtID)
        {
            var data = DataContext.Get();
            return data.Where(x => x.DistrictId == districtID).ToList();
        }
    }
}
