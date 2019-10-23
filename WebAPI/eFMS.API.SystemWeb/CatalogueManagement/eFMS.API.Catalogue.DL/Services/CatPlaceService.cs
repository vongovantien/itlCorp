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
using eFMS.API.Catalogue.DL.Common;
using eFMS.API.Catalogue.DL.ViewModels;
using System.Threading;
using System.Globalization;
using ITL.NetCore.Common;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Caching.Distributed;
using System.Data.SqlClient;
using eFMS.API.Catalogue.Service.Contexts;
using eFMS.API.Common.NoSql;
using eFMS.IdentityServer.DL.UserManager;
using eFMS.API.Common.Helpers;

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatPlaceService : RepositoryBase<CatPlace, CatPlaceModel>, ICatPlaceService
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly IDistributedCache cache;
        private readonly ICurrentUser currentUser;
        public CatPlaceService(IContextBase<CatPlace> repository, IMapper mapper, IStringLocalizer<LanguageSub> localizer, IDistributedCache distributedCache, ICurrentUser user) : base(repository, mapper)
        {
            stringLocalizer = localizer;
            cache = distributedCache;
            currentUser = user;
            SetChildren<CatCountry>("Id", "CountryId");
            SetChildren<CatPlace>("Id", "ProvinceId");
            SetChildren<CatPlace>("Id", "DistrictId");
            SetChildren<CsTransaction>("Id", "Pol");
            SetChildren<CsTransaction>("Id", "Pod");
            SetChildren<CsTransactionDetail>("Id", "Pol");
            SetChildren<CsTransactionDetail>("Id", "Pod");
            SetChildren<OpsTransaction>("Id", "Pol");
            SetChildren<OpsTransaction>("Id", "Pod");
            SetChildren<OpsTransaction>("Id", "WarehouseId");
            SetChildren<CsManifest>("Id", "Pol");
            SetChildren<CsManifest>("Id", "Pod");
            SetChildren<CsShippingInstruction>("Id", "Pod");
            SetChildren<CsShippingInstruction>("Id", "Pol");
        }

        #region CRUD
        public override HandleState Add(CatPlaceModel model)
        {
            var entity = mapper.Map<CatPlace>(model);
            entity.Id = Guid.NewGuid();
            entity.UserCreated = entity.UserModified = currentUser.UserID;
            entity.DatetimeCreated = entity.DatetimeModified = DateTime.Now;
            entity.Active = true;
            var result = DataContext.Add(entity, true);
            if (result.Success)
            {
                cache.Remove(Templates.CatPlace.NameCaching.ListName);
            }
            return result;
        }
        public HandleState Update(CatPlaceModel model)
        {
            var entity = mapper.Map<CatPlace>(model);
            entity.DatetimeModified = DateTime.Now;
            entity.UserModified = currentUser.UserID;
            if (entity.Active == false)
            {
                entity.InactiveOn = DateTime.Now;
            }
            var result = DataContext.Update(entity, x => x.Id == model.Id);
            if (result.Success)
            {
                cache.Remove(Templates.CatPlace.NameCaching.ListName);
            }
            return result;
        }
        public HandleState Delete(Guid id)
        {
            ChangeTrackerHelper.currentUser = currentUser.UserID;
            var hs = DataContext.Delete(x => x.Id == id);
            if (hs.Success)
            {
                cache.Remove(Templates.CatPlace.NameCaching.ListName);
            }
            return hs;
        }
        #endregion

        public IQueryable<CatPlaceModel> GetCatPlaces()
        {
            var places = RedisCacheHelper.GetObject<List<CatPlace>>(cache, Templates.CatPlace.NameCaching.ListName);
            IQueryable<CatPlace> data = null;
            if (places != null)
            {
                data = places.AsQueryable();
            }
            else
            {
                data = DataContext.Get();
                RedisCacheHelper.SetObject(cache, Templates.CatPlace.NameCaching.ListName, data);
            }
            var results = data?.Select(x => mapper.Map<CatPlaceModel>(x));
            return results;
        }

        public List<vw_catProvince> GetProvinces(short? countryId)
        {
            var data = GetProvinces().Where(x => x.CountryID == countryId || countryId == null).ToList();
            return data;
        }

        public List<CatPlaceViewModel> Paging(CatPlaceCriteria criteria, int page, int size, out int rowsCount)
        {
            List<CatPlaceViewModel> results = null;
            var data = Query(criteria);
            rowsCount = data.Count();
            if (rowsCount == 0) return results;
            if (size > 1)
            {
                data = data.OrderByDescending(x => x.DatetimeModified);
                if (page < 1)
                {
                    page = 1;
                }
                data = data.Skip((page-1)* size).Take(size);
            }
            results = GetCulturalData(data).ToList();
            return results;
        }

        public IQueryable<sp_GetCatPlace> Query(CatPlaceCriteria criteria)
        {
            string placetype = PlaceTypeEx.GetPlaceType(criteria.PlaceType);
            var list = GetBy(placetype);
            IQueryable<sp_GetCatPlace> results = null;
            if (criteria.All == null)
            {
                results = list.Where(x => ((x.Code ?? "").IndexOf(criteria.Code ?? "", StringComparison.OrdinalIgnoreCase) > -1)
                                    && ((x.NameEn ?? "").IndexOf(criteria.NameEn ?? "", StringComparison.OrdinalIgnoreCase) > -1)
                                    && ((x.NameVn ?? "").IndexOf(criteria.NameVn ?? "", StringComparison.OrdinalIgnoreCase) > -1)
                                    && ((x.CountryNameEN ?? "").IndexOf(criteria.CountryNameEN ?? "", StringComparison.OrdinalIgnoreCase) > -1)
                                    && ((x.CountryNameVN ?? "").IndexOf(criteria.CountryNameVN ?? "", StringComparison.OrdinalIgnoreCase) > -1)
                                    && ((x.DistrictNameEN ?? "").IndexOf(criteria.DistrictNameEN ?? "", StringComparison.OrdinalIgnoreCase) > -1)
                                    && ((x.DistrictNameVN ?? "").IndexOf(criteria.DistrictNameVN ?? "", StringComparison.OrdinalIgnoreCase) > -1)
                                    && (x.CountryID  == criteria.CountryId || criteria.CountryId==null)
                                    && (x.ProvinceID == criteria.ProvinceId || criteria.ProvinceId == null)
                                    && (x.DistrictID == criteria.DistrictId || criteria.DistrictId == null)
                                    && ((x.ProvinceNameEN ?? "").IndexOf(criteria.ProvinceNameEN ?? "", StringComparison.OrdinalIgnoreCase) > -1)
                                    && ((x.ProvinceNameVN ?? "").IndexOf(criteria.ProvinceNAmeVN ?? "", StringComparison.OrdinalIgnoreCase) > -1)
                                    && ((x.Address ?? "").IndexOf(criteria.Address ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                                    //&& ((x.PlaceTypeID ?? "").IndexOf(placetype ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                                    && ((x.ModeOfTransport ?? "").IndexOf(criteria.ModeOfTransport ?? "", StringComparison.OrdinalIgnoreCase) > -1)
                                    && (x.AreaNameEN ?? "").IndexOf(criteria.AreaNameEN ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                    && (x.AreaNameVN ?? "").IndexOf(criteria.AreaNameVN ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                    && (x.Active == criteria.Active || criteria.Active == null)
                    ).AsQueryable();
            }
            else
            {
                results = list.Where(x => (
                                      ((x.Code ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1)
                                   || ((x.NameEn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1)
                                   || ((x.NameVn ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1)
                                   || ((x.CountryNameEN ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1)
                                   || ((x.CountryNameVN ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1)
                                   || ((x.DistrictNameEN ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1)
                                   || ((x.DistrictNameVN ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1)
                                   || ((x.ProvinceNameEN ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1)
                                   || ((x.ProvinceNameVN ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1)
                                   || ((x.Address ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1)
                                   || (x.AreaNameEN ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                   || (x.AreaNameVN ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                   || ((x.ModeOfTransport ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) > -1)
                                   )
                                   //&& ((x.PlaceTypeID ?? "").IndexOf(placetype ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                                   && (x.Active == criteria.Active || criteria.Active == null)
                                   ).AsQueryable();
            }
            return results;
        }

        private IQueryable<sp_GetCatPlace> GetBy(string placeTypeID)
        {
            IQueryable<sp_GetCatPlace> data = null;
            //var data = GetView(placeTypeID);
            data = RedisCacheHelper.Get<sp_GetCatPlace>(cache, Templates.CatPlace.NameCaching.ListName);
            if (string.IsNullOrEmpty(placeTypeID))
            {
                if (data == null)
                {
                    data = GetView(placeTypeID).AsQueryable();
                    RedisCacheHelper.SetObject(cache, Templates.CatPlace.NameCaching.ListName, data);
                }
            }
            else
            {
                if(data == null)
                {
                    data = GetView(placeTypeID).AsQueryable();
                }
                else
                {
                    data = data.Where(x => x.PlaceTypeID == placeTypeID);
                }
            }
            return data;
        }
        private IQueryable<CatPlaceViewModel> GetCulturalData(IQueryable<sp_GetCatPlace> list)
        {
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            if (currentCulture.Name == "vi-VN")
            {
                 return list.Select(x => new CatPlaceViewModel
                {
                    ID = x.ID,
                    Code = x.Code,
                    NameEn = x.NameEn,
                    NameVn = x.NameVn,
                    DisplayName = x.DisplayName,
                    Address = x.Address,
                    DistrictID = x.DistrictID,
                    DistrictName = x.DistrictNameVN,
                    ProvinceID = x.ProvinceID,
                    ProvinceName = x.ProvinceNameVN,
                    CountryID = x.CountryID,
                    AreaID = x.AreaID,
                    LocalAreaID = x.LocalAreaID,
                    ModeOfTransport = x.ModeOfTransport,
                    GeoCode = x.GeoCode,
                    PlaceTypeID = x.PlaceTypeID,
                    Note = x.Note,
                    UserCreated = x.UserCreated,
                    DatetimeCreated = x.DatetimeCreated,
                    UserModified = x.UserModified,
                    DatetimeModified = x.DatetimeModified,
                    Active = x.Active,
                    InActiveOn = x.InActiveOn,
                    CountryName = x.CountryNameVN,
                    AreaName = x.AreaNameVN,
                    LocalAreaName = x.LocalAreaNameVN
                });
            }
            else
            {
                return list.Select(x => new CatPlaceViewModel
                {
                    ID = x.ID,
                    Code = x.Code,
                    NameEn = x.NameEn,
                    NameVn = x.NameVn,
                    DisplayName = x.DisplayName,
                    Address = x.Address,
                    DistrictID = x.DistrictID,
                    DistrictName = x.DistrictNameEN,
                    ProvinceID = x.ProvinceID,
                    ProvinceName = x.ProvinceNameEN,
                    CountryID = x.CountryID,
                    AreaID = x.AreaID,
                    LocalAreaID = x.LocalAreaID,
                    ModeOfTransport = x.ModeOfTransport,
                    GeoCode = x.GeoCode,
                    PlaceTypeID = x.PlaceTypeID,
                    Note = x.Note,
                    UserCreated = x.UserCreated,
                    DatetimeCreated = x.DatetimeCreated,
                    UserModified = x.UserModified,
                    DatetimeModified = x.DatetimeModified,
                    Active = x.Active,
                    InActiveOn = x.InActiveOn,
                    CountryName = x.CountryNameEN,
                    AreaName = x.AreaNameEN,
                    LocalAreaName = x.LocalAreaNameEN
                });
            }
        }

        private List<sp_GetCatPlace> GetView(string placeTypeID)
        {
            //List<vw_catPlace> lvCatPlace = ((eFMSDataContext)DataContext.DC).GetViewData<vw_catPlace>();
            //return lvCatPlace;
            var parameters = new[]{
                new SqlParameter(){ ParameterName="@placeTypeID", Value = placeTypeID }
            };
            var list = ((eFMSDataContext)DataContext.DC).ExecuteProcedure<sp_GetCatPlace>(parameters);
            return list;
        }

        private List<vw_catProvince> GetProvinces()
        {
            List<vw_catProvince> lvCatPlace = ((eFMSDataContext)DataContext.DC).GetViewData<vw_catProvince>();
            return lvCatPlace;
        }

        public List<vw_catDistrict> GetDistricts(Guid? provinceId)
        {
            var data = GetDistricts();
            return data.Where(x => x.ProvinceID == provinceId || provinceId == null).ToList();
        }

        private List<vw_catDistrict> GetDistricts()
        {
            List<vw_catDistrict> lvCatPlace = ((eFMSDataContext)DataContext.DC).GetViewData<vw_catDistrict>();
            return lvCatPlace;
        }

        public List<ModeOfTransport> GetModeOfTransport()
        {
            return DataEnums.ModeOfTransportData;
        }

        public List<CatPlaceImportModel> CheckValidImport(List<CatPlaceImportModel> list, CatPlaceTypeEnum placeType)
        {
            string placeTypeName = PlaceTypeEx.GetPlaceType(placeType);
            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            List<CatPlaceImportModel> results = null;
            switch (placeType)
            {
                case CatPlaceTypeEnum.Warehouse:
                    results = CheckWarehouseValidImport(list, dc, placeTypeName);
                    break;
                case CatPlaceTypeEnum.Port:
                    results = CheckPortIndexValidImport(list, dc, placeTypeName);
                    break;
                case CatPlaceTypeEnum.Province:
                    results = CheckProvinceValidImport(list, dc, placeTypeName);
                    break;
                case CatPlaceTypeEnum.District:
                    results = CheckDistrictValidImport(list, dc, placeTypeName);
                    break;
                case CatPlaceTypeEnum.Ward:
                    results = CheckWardValidImport(list, dc, placeTypeName);
                    break;
            }
            return results;
        }

        private List<CatPlaceImportModel> CheckWardValidImport(List<CatPlaceImportModel> list, eFMSDataContext dc, string placeTypeName)
        {
            var countries = dc.CatCountry.ToList();
            var provinces = dc.CatPlace.Where(x => x.PlaceTypeId == PlaceTypeEx.GetPlaceType(CatPlaceTypeEnum.Province)).ToList();
            var districts = dc.CatPlace.Where(x => x.PlaceTypeId == PlaceTypeEx.GetPlaceType(CatPlaceTypeEnum.District)).ToList();
            var wards = dc.CatPlace.Where(x => x.PlaceTypeId == placeTypeName);
            var results = new List<CatPlaceImportModel>();
            list.ForEach(item =>
            {
                item.PlaceTypeId = placeTypeName;

                if (string.IsNullOrEmpty(item.NameEn))
                {
                    item.NameEn = stringLocalizer[LanguageSub.MSG_PLACE_NAME_EN_EMPTY];
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.NameVn))
                {
                    item.NameVn = stringLocalizer[LanguageSub.MSG_PLACE_NAME_LOCAL_EMPTY];
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.Code))
                {
                    item.Code = stringLocalizer[LanguageSub.MSG_PLACE_CODE_EMPTY];
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.CountryName))
                {
                    item.CountryName = stringLocalizer[LanguageSub.MSG_PLACE_COUNTRY_NAME_EMPTY];
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.ProvinceName))
                {
                    item.ProvinceName = stringLocalizer[LanguageSub.MSG_PLACE_PROVINCE_NAME_EMPTY];
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.DistrictName))
                {
                    item.DisplayName = stringLocalizer[LanguageSub.MSG_PLACE_DISTRICT_NAME_EMPTY];
                    item.IsValid = false;
                }
                else
                {
                    var country = countries.FirstOrDefault(i => i.NameEn.ToLower() == item.CountryName.ToLower());

                    if (country == null)
                    {
                        item.CountryName = string.Format(stringLocalizer[LanguageSub.MSG_PLACE_COUNTRY_NOT_FOUND], item.CountryName);
                        item.IsValid = false;
                    }
                    else
                    {
                        item.CountryId = country.Id;
                        var province = provinces.FirstOrDefault(i => i.NameEn.ToLower() == item.ProvinceName.ToLower() && (i.CountryId == country.Id || country == null));
                        if (province == null)
                        {
                            item.ProvinceName = string.Format(stringLocalizer[LanguageSub.MSG_PLACE_PROVINCE_NOT_FOUND], item.ProvinceName, item.CountryName);
                            item.IsValid = false;
                        }
                        else
                        {
                            item.ProvinceId = province.Id;
                            var district = districts.FirstOrDefault(i => i.NameEn.ToLower() == item.DistrictName.ToLower() && i.ProvinceId == province.Id);
                            if (district == null)
                            {
                                item.DistrictName = string.Format(stringLocalizer[LanguageSub.MSG_PLACE_DISTRICT_NOT_FOUND], item.DistrictName, item.ProvinceName);
                                item.IsValid = false;
                            }
                            else
                            {
                                item.DistrictId = district.Id;
                            }
                            if (list.Count(x => x.Code.ToLower() == item.Code.ToLower()) > 1)
                            {
                                item.Code = string.Format(stringLocalizer[LanguageSub.MSG_PLACE_CODE_DUPLICATE], item.Code);
                                item.IsValid = false;
                            }
                            var ward = wards.FirstOrDefault(x => x.Code.ToLower() == item.Code.ToLower());
                            if (ward != null)
                            {
                                item.Code = string.Format(stringLocalizer[LanguageSub.MSG_PLACE_CODE_EXISTED], item.Code);
                                item.IsValid = false;
                            }
                        }
                    }
                }
            });
            return list;
        }

        private List<CatPlaceImportModel> CheckDistrictValidImport(List<CatPlaceImportModel> list, eFMSDataContext dc, string placeTypeName)
        {
            var countries = dc.CatCountry.ToList();
            var provinces = dc.CatPlace.Where(x => x.PlaceTypeId == PlaceTypeEx.GetPlaceType(CatPlaceTypeEnum.Province)).ToList();
            var districts = dc.CatPlace.Where(x => x.PlaceTypeId == placeTypeName).ToList();
            list.ForEach(item =>
            {
                item.PlaceTypeId = placeTypeName;
                if (string.IsNullOrEmpty(item.Code))
                {
                    item.Code = stringLocalizer[LanguageSub.MSG_PLACE_CODE_EMPTY];
                    item.IsValid = false;
                }

                if (string.IsNullOrEmpty(item.NameEn))
                {
                    item.NameEn = stringLocalizer[LanguageSub.MSG_PLACE_NAME_EN_EMPTY];
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.NameVn))
                {
                    item.NameVn = stringLocalizer[LanguageSub.MSG_PLACE_NAME_LOCAL_EMPTY];
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.CountryName))
                {
                    item.CountryName = stringLocalizer[LanguageSub.MSG_PLACE_COUNTRY_NAME_EMPTY];
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.ProvinceName))
                {
                    item.ProvinceName = stringLocalizer[LanguageSub.MSG_PLACE_PROVINCE_NAME_EMPTY];
                    item.IsValid = false;
                }
                else
                {
                    var country = countries.FirstOrDefault(i => i.NameEn.ToLower() == item.CountryName.ToLower());
                    if (country == null)
                    {
                        item.CountryName = string.Format(stringLocalizer[LanguageSub.MSG_PLACE_COUNTRY_NOT_FOUND], item.CountryName);
                        item.IsValid = false;
                    }
                    else
                    {
                        item.CountryId = country.Id;
                        var province = provinces.FirstOrDefault(i => i.NameEn.ToLower() == item.ProvinceName.ToLower() && (i.CountryId == country.Id || country == null));

                        if (province == null)
                        {
                            item.ProvinceName = string.Format(stringLocalizer[LanguageSub.MSG_PLACE_PROVINCE_NOT_FOUND], item.ProvinceName, item.CountryName);
                            item.IsValid = false;
                        }
                        else
                        {
                            item.ProvinceId = province.Id;

                            var district = districts.FirstOrDefault(i => i.Code.ToLower() == item.Code.ToLower());
                            if (district != null)
                            {
                                item.Code = string.Format(stringLocalizer[LanguageSub.MSG_PLACE_CODE_EXISTED], item.Code);
                                item.IsValid = false;
                            }
                            else
                            {
                                var countNew = list.Count(i => i.Code.ToLower() == item.Code.ToLower());
                                if(countNew > 1)
                                {
                                    item.Code = string.Format(stringLocalizer[LanguageSub.MSG_PLACE_CODE_DUPLICATE], item.Code);
                                    item.IsValid = false;
                                }
                            }
                        }
                    }
                }
            });
            return list;
        }

        private List<CatPlaceImportModel> CheckProvinceValidImport(List<CatPlaceImportModel> list, eFMSDataContext dc, string placeTypeName)
        {
            var countries = dc.CatCountry;
            var provinces = dc.CatPlace.Where(x => x.PlaceTypeId == placeTypeName).ToList();
            var results = new List<CatPlaceImportModel>();
            foreach (var item in list)
            {
                var result = CheckCatplaceValidImport(provinces, results, item);
                result.PlaceTypeId = placeTypeName;
                if (string.IsNullOrEmpty(item.CountryName))
                {
                    result.CountryName = stringLocalizer[LanguageSub.MSG_PLACE_COUNTRY_NAME_EMPTY];
                    result.IsValid = false;
                }
                else
                {
                    var country = countries.FirstOrDefault(i => i.NameEn.ToLower() == item.CountryName.ToLower());
                    if (country == null)
                    {
                        result.CountryName = string.Format(stringLocalizer[LanguageSub.MSG_PLACE_COUNTRY_NOT_FOUND], item.CountryName);
                        result.IsValid = false;
                    }
                    else
                    {
                        result.CountryId = country.Id;
                    }
                }
                results.Add(result);
            }
            return results;
        }

        private CatPlaceImportModel CheckCatplaceValidImport(List<CatPlace> places, List<CatPlaceImportModel> newList, CatPlaceImportModel item)
        {
            if (string.IsNullOrEmpty(item.Code))
            {
                item.Code = stringLocalizer[LanguageSub.MSG_PLACE_CODE_EMPTY];
                item.IsValid = false;
            }
            else if (newList.Count(x => (x.Code ?? "").ToLower() == item.Code.ToLower()) > 0)
            {
                var existedItemDuplicate = newList.FirstOrDefault(x => (x.Code ?? "").ToLower() == item.Code.ToLower());
                if(existedItemDuplicate != null)
                {
                    existedItemDuplicate.Code = string.Format(stringLocalizer[LanguageSub.MSG_PLACE_CODE_DUPLICATE], item.Code);
                    existedItemDuplicate.IsValid = false;
                    newList[newList.FindIndex(x => x.Id == existedItemDuplicate.Id)] = existedItemDuplicate;
                    item.Code = string.Format(stringLocalizer[LanguageSub.MSG_PLACE_CODE_DUPLICATE], item.Code);
                    item.IsValid = false;
                }
            }
            else
            {
                if(places.Any(i => (i.Code ?? "").ToLower() == item.Code.ToLower()))
                {
                    item.Code = string.Format(stringLocalizer[LanguageSub.MSG_PLACE_CODE_EXISTED], item.Code);
                    item.IsValid = false;
                }
            }
            if (string.IsNullOrEmpty(item.NameEn))
            {
                item.NameEn = stringLocalizer[LanguageSub.MSG_PLACE_NAME_EN_EMPTY];
                item.IsValid = false;
            }
            if (string.IsNullOrEmpty(item.NameVn))
            {
                item.NameVn = stringLocalizer[LanguageSub.MSG_PLACE_NAME_LOCAL_EMPTY];
                item.IsValid = false;
            }
            return item;
        }

        private List<CatPlaceImportModel> CheckPortIndexValidImport(List<CatPlaceImportModel> list, eFMSDataContext dc, string placeTypeName)
        {
            var countries = dc.CatCountry;
            var areas = dc.CatArea.ToList();
            var modes = DataEnums.ModeOfTransportData;
            var portIndexs = dc.CatPlace.Where(x => x.PlaceTypeId == placeTypeName).ToList();
            var results = new List<CatPlaceImportModel>();
            foreach(var item in list)
            {
                var result = CheckCatplaceValidImport(portIndexs, results, item);
                result.PlaceTypeId = placeTypeName;
                if (string.IsNullOrEmpty(item.CountryName))
                {
                    result.CountryName = stringLocalizer[LanguageSub.MSG_PLACE_COUNTRY_NAME_EMPTY];
                    result.IsValid = false;
                }
                else
                {
                    var country = countries.FirstOrDefault(i => (i.NameEn ??"") == item.CountryName.ToLower());
                    if (country == null)
                    {
                        result.CountryName = string.Format(stringLocalizer[LanguageSub.MSG_PLACE_COUNTRY_NOT_FOUND], item.CountryName);
                        result.IsValid = false;
                    }
                    else
                    {
                        result.CountryId = country.Id;
                    }
                }
                if (string.IsNullOrEmpty(item.ModeOfTransport))
                {
                    result.ModeOfTransport = stringLocalizer[LanguageSub.MSG_PLACE_PORTINDEX_MODE_EMPTY];
                    result.IsValid = false;
                }
                else
                {
                    if(DataEnums.ModeOfTransportData.Any(x => x.Id == item.ModeOfTransport.ToUpper())){
                        result.ModeOfTransport = item.ModeOfTransport.ToUpper();
                    }
                    else
                    {
                        result.ModeOfTransport = string.Format(stringLocalizer[LanguageSub.MSG_PLACE_PORTINDEX_MODE_NOT_FOUND], item.ModeOfTransport);
                        result.IsValid = false;
                    }
                }
                if (!string.IsNullOrEmpty(item.AreaName))
                {
                    var area = areas.FirstOrDefault(i => i.NameEn.ToLower() == item.AreaName.ToLower());
                    if (area == null)
                    {
                        result.AreaName = string.Format(stringLocalizer[LanguageSub.MSG_PLACE_PORTINDEX_AREA_NOT_FOUND], item.CountryName);
                        result.IsValid = false;
                    }
                    else
                    {
                        result.AreaId = area.Id;
                    }
                }
                else
                {
                    result.AreaName = string.Empty;
                }
                results.Add(result);
            }
            return results;
        }

        private List<CatPlaceImportModel> CheckWarehouseValidImport(List<CatPlaceImportModel> list, eFMSDataContext dc, string placeTypeName)
        {
            var countries = dc.CatCountry.ToList();
            var provinces = dc.CatPlace.Where(x => x.PlaceTypeId == PlaceTypeEx.GetPlaceType(CatPlaceTypeEnum.Province)).ToList();
            var districts = dc.CatPlace.Where(x => x.PlaceTypeId == PlaceTypeEx.GetPlaceType(CatPlaceTypeEnum.District)).ToList();
            var warehouses = dc.CatPlace.Where(x => x.PlaceTypeId == placeTypeName).ToList();
            var results = new List<CatPlaceImportModel>();
            foreach (var item in list)
            {
                var result = CheckCatplaceValidImport(warehouses, results, item);
                result.PlaceTypeId = placeTypeName;
                if (string.IsNullOrEmpty(item.Address))
                {
                    result.Address = stringLocalizer[LanguageSub.MSG_PLACE_ADDRESS_EMPTY];
                    result.IsValid = false;
                }
                else
                {
                    result.Address = item.Address;
                }
                if (string.IsNullOrEmpty(item.CountryName))
                {
                    result.CountryName = stringLocalizer[LanguageSub.MSG_PLACE_COUNTRY_NAME_EMPTY];
                    result.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.ProvinceName))
                {
                    result.ProvinceName = stringLocalizer[LanguageSub.MSG_PLACE_PROVINCE_NAME_EMPTY];
                    result.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.DistrictName))
                {
                    result.DistrictName = stringLocalizer[LanguageSub.MSG_PLACE_DISTRICT_NAME_EMPTY];
                    result.IsValid = false;
                }
                else
                {
                    var country = countries.FirstOrDefault(i => i.NameEn.ToLower() == item.CountryName.ToLower());
                    if (country == null)
                    {
                        result.CountryName = string.Format(stringLocalizer[LanguageSub.MSG_PLACE_COUNTRY_NOT_FOUND], item.CountryName);
                        result.IsValid = false;
                    }
                    else
                    {
                        result.CountryId = country.Id;
                        var province = provinces.FirstOrDefault(i => i.NameEn.ToLower() == item.ProvinceName.ToLower() && (i.CountryId == country.Id || country == null));
                        if (province == null)
                        {
                            result.ProvinceName = string.Format(stringLocalizer[LanguageSub.MSG_PLACE_PROVINCE_NOT_FOUND], item.ProvinceName, item.CountryName);
                            result.IsValid = false;
                        }
                        else
                        {
                            result.ProvinceId = province.Id;
                            var district = districts.FirstOrDefault(i => i.NameEn.ToLower() == item.DistrictName.ToLower() && (i.ProvinceId == province.Id || province == null));
                            if (district == null)
                            {
                                result.DistrictName = string.Format(stringLocalizer[LanguageSub.MSG_PLACE_DISTRICT_NOT_FOUND], item.DistrictName, item.ProvinceName);
                                result.IsValid = false;
                            }
                            else
                            {
                                result.DistrictId = district.Id;
                            }
                        }
                    }
                }
                result.PlaceTypeId = placeTypeName;
                results.Add(result);
            }
            return results;
        }
        
        public HandleState Import(List<CatPlaceImportModel> data)
        {
            try
            {
                eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
                foreach (var item in data)
                {
                    bool Active = string.IsNullOrEmpty(item.Status) ? false : (item.Status.Trim().ToLower() == "Active" ? true : false);
                    DateTime? ActiveDate = Active == false ? null : (DateTime?)DateTime.Now;
                    var catPlace = new CatPlace
                    {   Id = Guid.NewGuid(),
                        Code = item.Code,
                        NameEn = item.NameEn,
                        NameVn = item.NameVn,
                        CountryId = item.CountryId,
                        ProvinceId = item.ProvinceId,
                        DistrictId = item.DistrictId,
                        Address = item.Address,
                        DatetimeCreated = DateTime.Now,
                        UserCreated = currentUser.UserID,
                        UserModified = currentUser.UserID,
                        PlaceTypeId = item.PlaceTypeId,
                        Active = Active,
                        InactiveOn = ActiveDate,
                        ModeOfTransport = item.ModeOfTransport,
                        AreaId = item.AreaId
                    };
                    dc.CatPlace.Add(catPlace);
                }
                dc.SaveChanges();
                cache.Remove(Templates.CatPlace.NameCaching.ListName);
                return new HandleState();
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }

    }
}
