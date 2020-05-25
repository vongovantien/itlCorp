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
using System.Data.SqlClient;
using eFMS.API.Catalogue.Service.Contexts;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Connection.Caching;
using eFMS.API.Infrastructure.Extensions;
using eFMS.API.Common.Models;

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatPlaceService : RepositoryBaseCache<CatPlace, CatPlaceModel>, ICatPlaceService
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICurrentUser currentUser;
        //private readonly ICatCountryService countryService;
        private readonly IContextBase<CatCountry> countryRepository;
        private readonly IContextBase<CatArea> areaRepository;

        public CatPlaceService(IContextBase<CatPlace> repository,
            ICacheServiceBase<CatPlace> cacheService,
            IMapper mapper,
            IStringLocalizer<CatalogueLanguageSub> localizer,
            ICurrentUser user,
            //ICatCountryService country,
            IContextBase<CatArea> areaRepo,
            IContextBase<CatCountry> countryRepo) : base(repository, cacheService, mapper)
        {
            stringLocalizer = localizer;
            currentUser = user;
            //countryService = country;
            areaRepository = areaRepo;
            countryRepository = countryRepo;
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
            //SetChildren<CsShippingInstruction>("Id", "Pod");
            //SetChildren<CsShippingInstruction>("Id", "Pol");
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
                ClearCache();
                Get();
            }
            return result;
        }
        public HandleState Update(CatPlaceModel model)
        {
            var entity = GetModelToUpdate(model);
            var result = DataContext.Update(entity, x => x.Id == model.Id);
            if (result.Success)
            {
                ClearCache();
                Get();
            }
            return result;
        }

        private CatPlace GetModelToUpdate(CatPlaceModel model)
        {
            var entity = mapper.Map<CatPlace>(model);
            var place = DataContext.Get(x => x.Id == model.Id).FirstOrDefault();
            entity.DatetimeModified = DateTime.Now;
            entity.UserModified = currentUser.UserID;
            entity.GroupId = place.GroupId;
            entity.DepartmentId = place.DepartmentId;
            entity.OfficeId = place.OfficeId;
            entity.CompanyId = place.CompanyId;

            if (entity.Active == false)
            {
                entity.InactiveOn = DateTime.Now;
            }
            return entity;
        }

        public HandleState Delete(Guid id)
        {
            var hs = DataContext.Delete(x => x.Id == id);
            if (hs.Success)
            {
                ClearCache();
                Get();
            }
            return hs;
        }
        #endregion

        public IQueryable<CatPlaceModel> GetCatPlaces()
        {
            var results = Get();
            return results;
        }

        public List<vw_catProvince> GetProvinces(short? countryId)
        {
            var data = GetProvinces().Where(x => x.CountryID == countryId || countryId == null).ToList();
            return data;
        }

        public List<CatPlaceViewModel> Paging(CatPlaceCriteria criteria, int page, int size, out int rowsCount)
        {
            IQueryable<sp_GetCatPlace> data = null;
            List<CatPlaceViewModel> results = null;

            if (criteria.PlaceType == CatPlaceTypeEnum.Warehouse)
            {
                ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.catWarehouse);
                var rangeSearch = PermissionExtention.GetPermissionRange(currentUser.UserMenuPermission.List);
                if (rangeSearch == PermissionRange.None)
                {
                    rowsCount = 0;
                    return null;
                }
                data = QueryByPermission(criteria, rangeSearch);
            }
            else if (criteria.PlaceType == CatPlaceTypeEnum.Port)
            {
                ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.catPortindex);
                var rangeSearch = PermissionExtention.GetPermissionRange(currentUser.UserMenuPermission.List);
                if (rangeSearch == PermissionRange.None)
                {
                    rowsCount = 0;
                    return null;
                }
                data = QueryByPermission(criteria, rangeSearch);
            }
            else
            {
                data = Query(criteria);

            }

            rowsCount = data.Select(x => x.ID).Count();
            if (rowsCount == 0) return results;
            if (size > 1)
            {
                data = data.OrderByDescending(x => x.DatetimeModified);
                if (page < 1)
                {
                    page = 1;
                }
                data = data.Skip((page - 1) * size).Take(size);
            }
            results = GetCulturalData(data).ToList();
            return results;
        }

        public IQueryable<sp_GetCatPlace> Get(CatPlaceCriteria criteria)
        {
            var countries = countryRepository.Get();

            string placetype = PlaceTypeEx.GetPlaceType(criteria.PlaceType);

            // Join left với country.
            var places = from x in DataContext.Get(x => (x.PlaceTypeId == placetype || string.IsNullOrEmpty(placetype)
                                            && (x.Active == criteria.Active || criteria.Active == null))
                                            && (x.ModeOfTransport.IndexOf(criteria.ModeOfTransport, StringComparison.OrdinalIgnoreCase) > -1) || string.IsNullOrEmpty(criteria.ModeOfTransport))
                         join coun in countries on x.CountryId equals coun.Id into coun2
                         from coun in coun2.DefaultIfEmpty()
                         select new sp_GetCatPlace
                         {
                             ID = x.Id,
                             Code = x.Code,
                             NameVn = x.NameVn,
                             NameEn = x.NameEn,
                             DisplayName = x.DisplayName,
                             Address = x.Address,
                             DistrictID = x.DistrictId,
                             DistrictNameEN = string.Empty,
                             DistrictNameVN = string.Empty,
                             ProvinceID = x.ProvinceId,
                             ProvinceNameEN = string.Empty,
                             ProvinceNameVN = string.Empty,
                             CountryNameEN = coun.NameEn,
                             CountryNameVN = coun.NameVn,
                             CountryID = x.CountryId,
                             AreaID = x.AreaId,
                             LocalAreaID = x.LocalAreaId,
                             ModeOfTransport = x.ModeOfTransport,
                             PlaceTypeID = x.PlaceTypeId,
                             Note = x.Note,
                             UserCreated = x.UserCreated,
                             DatetimeCreated = x.DatetimeCreated,
                             UserModified = x.UserModified,
                             DatetimeModified = x.DatetimeModified,
                             Active = x.Active
                         };

            return places;
        }
        public IQueryable<sp_GetCatPlace> Query(CatPlaceCriteria criteria)
        {
            return QueryCriteria(criteria);
        }
        private IQueryable<sp_GetCatPlace> GetBy(string placeTypeID)
        {
            var data = GetView(placeTypeID).AsQueryable();
            return data;
        }

        private IQueryable<CatPlaceViewModel> GetCulturalData(IQueryable<sp_GetCatPlace> list)
        {
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            return list.Select(x => new CatPlaceViewModel
            {
                ID = x.ID,
                Code = x.Code,
                NameEn = x.NameEn,
                NameVn = x.NameVn,
                DisplayName = x.DisplayName,
                Address = x.Address,
                DistrictID = x.DistrictID,
                DistrictName = currentCulture.IetfLanguageTag == "en-US" ? x.DistrictNameEN : x.DistrictNameVN,
                ProvinceID = x.ProvinceID,
                ProvinceName = currentCulture.IetfLanguageTag == "en-US" ? x.ProvinceNameEN : x.ProvinceNameVN,
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
                CountryName = currentCulture.IetfLanguageTag == "en-US" ? x.CountryNameEN : x.CountryNameVN,
                AreaName = x.AreaNameVN,
                LocalAreaName = x.LocalAreaNameVN,
                FlightVesselNo = x.FlightVesselNo,
                
            });
        }

        public IQueryable<sp_GetCatPlace> QueryByPermission(CatPlaceCriteria criteria, PermissionRange range)
        {
            var list = QueryCriteria(criteria);
            if (list == null) return null;
            IQueryable<sp_GetCatPlace> data = null;
            if(list == null)
            {
                return null;
            }
            switch (range)
            {
                case PermissionRange.Owner:
                    data = list.Where(x => x.UserCreated == currentUser.UserID);
                    break;
                case PermissionRange.Group:
                    data = list.Where(x => x.UserCreated == currentUser.UserID
                    || x.GroupId == currentUser.GroupId
                    && x.DepartmentId == currentUser.DepartmentId
                    && x.OfficeId == currentUser.OfficeID
                    && x.CompanyId == currentUser.CompanyID);
                    break;
                case PermissionRange.Department:
                    data = list.Where(x => x.UserCreated == currentUser.UserID || x.DepartmentId == currentUser.DepartmentId && x.OfficeId == currentUser.OfficeID
                    && x.CompanyId == currentUser.CompanyID);
                    break;
                case PermissionRange.Office:
                    data = list.Where(x => x.UserCreated == currentUser.UserID || x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID);
                    break;
                case PermissionRange.Company:
                    data = list.Where(x => x.UserCreated == currentUser.UserID || x.CompanyId == currentUser.CompanyID);
                    break;
                case PermissionRange.All:
                    data = list;
                    break;
                default:
                    break;
            }

            return data;
        }

        private List<sp_GetCatPlace> GetView(string placeTypeID)
        {
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
            List<CatPlaceImportModel> results = null;
            switch (placeType)
            {
                case CatPlaceTypeEnum.Warehouse:
                    results = CheckWarehouseValidImport(list, placeTypeName);
                    break;
                case CatPlaceTypeEnum.Port:
                    results = CheckPortIndexValidImport(list, placeTypeName);
                    break;
                case CatPlaceTypeEnum.Province:
                    results = CheckProvinceValidImport(list, placeTypeName);
                    break;
                case CatPlaceTypeEnum.District:
                    results = CheckDistrictValidImport(list, placeTypeName);
                    break;
                case CatPlaceTypeEnum.Ward:
                    results = CheckWardValidImport(list, placeTypeName);
                    break;
            }
            return results;
        }

        private List<CatPlaceImportModel> CheckWardValidImport(List<CatPlaceImportModel> list, string placeTypeName)
        {
            var countries = countryRepository.Get().ToList();
            var places = Get();
            var provinces = places.Where(x => x.PlaceTypeId == PlaceTypeEx.GetPlaceType(CatPlaceTypeEnum.Province));
            var districts = places.Where(x => x.PlaceTypeId == PlaceTypeEx.GetPlaceType(CatPlaceTypeEnum.District)).ToList();
            var wards = places.Where(x => x.PlaceTypeId == placeTypeName);
            var results = new List<CatPlaceImportModel>();
            list.ForEach(item =>
            {
                item.PlaceTypeId = placeTypeName;

                if (string.IsNullOrEmpty(item.NameEn))
                {
                    item.NameEnError = stringLocalizer[CatalogueLanguageSub.MSG_PLACE_NAME_EN_EMPTY];
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.NameVn))
                {
                    item.NameVnError = stringLocalizer[CatalogueLanguageSub.MSG_PLACE_NAME_LOCAL_EMPTY];
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.Code))
                {
                    item.CodeError = stringLocalizer[CatalogueLanguageSub.MSG_PLACE_CODE_EMPTY];
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.CountryName))
                {
                    item.CountryNameError = stringLocalizer[CatalogueLanguageSub.MSG_PLACE_COUNTRY_NAME_EMPTY];
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.ProvinceName))
                {
                    item.ProvinceNameError = stringLocalizer[CatalogueLanguageSub.MSG_PLACE_PROVINCE_NAME_EMPTY];
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.DistrictName))
                {
                    item.DisplayNameError = stringLocalizer[CatalogueLanguageSub.MSG_PLACE_DISTRICT_NAME_EMPTY];
                    item.IsValid = false;
                }
                else
                {
                    var country = countries.FirstOrDefault(i => i.NameEn.ToLower() == item.CountryName.ToLower());

                    if (country == null)
                    {
                        item.CountryNameError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PLACE_COUNTRY_NOT_FOUND], item.CountryName);
                        item.IsValid = false;
                    }
                    else
                    {
                        item.CountryId = country.Id;
                        var province = provinces.FirstOrDefault(i => i.NameEn.ToLower() == item.ProvinceName.ToLower() && (i.CountryId == country.Id || country == null));
                        if (province == null)
                        {
                            item.ProvinceNameError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PLACE_PROVINCE_NOT_FOUND], item.ProvinceName, item.CountryName);
                            item.IsValid = false;
                        }
                        else
                        {
                            item.ProvinceId = province.Id;
                            var district = districts.FirstOrDefault(i => i.NameEn.ToLower() == item.DistrictName.ToLower() && i.ProvinceId == province.Id);
                            if (district == null)
                            {
                                item.DistrictNameError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PLACE_DISTRICT_NOT_FOUND], item.DistrictName, item.ProvinceName);
                                item.IsValid = false;
                            }
                            else
                            {
                                item.DistrictId = district.Id;
                            }
                            if (list.Count(x => x.Code.ToLower() == item.Code.ToLower()) > 1)
                            {
                                item.CodeError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PLACE_CODE_DUPLICATE], item.Code);
                                item.IsValid = false;
                            }
                            var ward = wards.FirstOrDefault(x => x.Code.ToLower() == item.Code.ToLower());
                            if (ward != null)
                            {
                                item.CodeError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PLACE_CODE_EXISTED], item.Code);
                                item.IsValid = false;
                            }
                        }
                    }
                }
            });
            return list;
        }

        private List<CatPlaceImportModel> CheckDistrictValidImport(List<CatPlaceImportModel> list, string placeTypeName)
        {
            var countries = countryRepository.Get();
            var places = Get();
            var provinces = places.Where(x => x.PlaceTypeId == PlaceTypeEx.GetPlaceType(CatPlaceTypeEnum.Province)).ToList();
            var districts = places.Where(x => x.PlaceTypeId == placeTypeName).ToList();
            list.ForEach(item =>
            {
                item.PlaceTypeId = placeTypeName;
                if (string.IsNullOrEmpty(item.Code))
                {
                    item.CodeError = stringLocalizer[CatalogueLanguageSub.MSG_PLACE_CODE_EMPTY];
                    item.IsValid = false;
                }

                if (string.IsNullOrEmpty(item.NameEn))
                {
                    item.NameEnError = stringLocalizer[CatalogueLanguageSub.MSG_PLACE_NAME_EN_EMPTY];
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.NameVn))
                {
                    item.NameVnError = stringLocalizer[CatalogueLanguageSub.MSG_PLACE_NAME_LOCAL_EMPTY];
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.CountryName))
                {
                    item.CountryNameError = stringLocalizer[CatalogueLanguageSub.MSG_PLACE_COUNTRY_NAME_EMPTY];
                    item.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.ProvinceName))
                {
                    item.ProvinceNameError = stringLocalizer[CatalogueLanguageSub.MSG_PLACE_PROVINCE_NAME_EMPTY];
                    item.IsValid = false;
                }
                else
                {
                    if (!string.IsNullOrEmpty(item.CountryName))
                    {
                        var country = countries.FirstOrDefault(i => i.NameEn.ToLower() == item.CountryName.ToLower());
                        if (country == null)
                        {
                            item.CountryNameError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PLACE_COUNTRY_NOT_FOUND], item.CountryName);
                            item.IsValid = false;
                        }
                        else
                        {
                            item.CountryId = country.Id;
                            var province = provinces.FirstOrDefault(i => i.NameEn.ToLower() == item.ProvinceName.ToLower() && (i.CountryId == country.Id || country == null));

                            if (province == null)
                            {
                                item.ProvinceNameError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PLACE_PROVINCE_NOT_FOUND], item.ProvinceName, item.CountryName);
                                item.IsValid = false;
                            }
                            else
                            {
                                item.ProvinceId = province.Id;

                                var district = districts.FirstOrDefault(i => i.Code.ToLower() == item.Code.ToLower());
                                if (district != null)
                                {
                                    item.CodeError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PLACE_CODE_EXISTED], item.Code);
                                    item.IsValid = false;
                                }
                                else
                                {
                                    var countNew = list.Count(i => i.Code.ToLower() == item.Code.ToLower());
                                    if (countNew > 1)
                                    {
                                        item.CodeError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PLACE_CODE_DUPLICATE], item.Code);
                                        item.IsValid = false;
                                    }
                                }
                            }
                        }
                    }
                }
            });
            return list;
        }

        private List<CatPlaceImportModel> CheckProvinceValidImport(List<CatPlaceImportModel> list, string placeTypeName)
        {
            var countries = countryRepository.Get().ToList();
            var provinces = DataContext.Get(x => x.PlaceTypeId == placeTypeName).ToList();
            var results = new List<CatPlaceImportModel>();
            foreach (var item in list)
            {
                var result = CheckCatplaceValidImport(provinces, results, item);
                result.PlaceTypeId = placeTypeName;
                if (string.IsNullOrEmpty(item.CountryName))
                {
                    result.CountryNameError = stringLocalizer[CatalogueLanguageSub.MSG_PLACE_COUNTRY_NAME_EMPTY];
                    result.IsValid = false;
                }
                else
                {
                    var country = countries.FirstOrDefault(i => i.NameEn.IndexOf(item.CountryName, StringComparison.OrdinalIgnoreCase) > -1);
                    if (country == null)
                    {
                        result.CountryNameError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PLACE_COUNTRY_NOT_FOUND], item.CountryName);
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
                item.CodeError = stringLocalizer[CatalogueLanguageSub.MSG_PLACE_CODE_EMPTY];
                item.IsValid = false;
            }
            else if (newList.Count(x => (x.Code ?? "").ToLower() == item.Code.ToLower()) > 0)
            {
                var existedItemDuplicate = newList.FirstOrDefault(x => (x.Code ?? "").ToLower() == item.Code.ToLower());
                if (existedItemDuplicate != null)
                {
                    existedItemDuplicate.CodeError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PLACE_CODE_DUPLICATE], item.Code);
                    existedItemDuplicate.IsValid = false;
                    newList[newList.FindIndex(x => x.Id == existedItemDuplicate.Id)] = existedItemDuplicate;
                    item.CodeError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PLACE_CODE_DUPLICATE], item.Code);
                    item.IsValid = false;
                }
            }
            else
            {
                if (places.Any(i => (i.Code ?? "").ToLower() == item.Code.ToLower()))
                {
                    item.CodeError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PLACE_CODE_EXISTED], item.Code);
                    item.IsValid = false;
                }
            }
            if (string.IsNullOrEmpty(item.NameEn))
            {
                item.NameEnError = stringLocalizer[CatalogueLanguageSub.MSG_PLACE_NAME_EN_EMPTY];
                item.IsValid = false;
            }
            if (string.IsNullOrEmpty(item.NameVn))
            {
                item.NameVnError = stringLocalizer[CatalogueLanguageSub.MSG_PLACE_NAME_LOCAL_EMPTY];
                item.IsValid = false;
            }
            return item;
        }

        private List<CatPlaceImportModel> CheckPortIndexValidImport(List<CatPlaceImportModel> list, string placeTypeName)
        {
            var countries = countryRepository.Get();
            var areas = areaRepository.Get().ToList();
            var modes = DataEnums.ModeOfTransportData;
            var portIndexs = DataContext.Get(x => x.PlaceTypeId == placeTypeName).ToList();
            var results = new List<CatPlaceImportModel>();
            foreach (var item in list)
            {
                var result = CheckCatplaceValidImport(portIndexs, results, item);
                result.PlaceTypeId = placeTypeName;
                if (string.IsNullOrEmpty(item.CountryName))
                {
                    result.CountryNameError = stringLocalizer[CatalogueLanguageSub.MSG_PLACE_COUNTRY_NAME_EMPTY];
                    result.IsValid = false;
                }
                else
                {
                    var country = countries.FirstOrDefault(i => (i.NameEn ?? "").ToLower() == item.CountryName.ToLower());
                    if (country == null)
                    {
                        result.CountryNameError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PLACE_COUNTRY_NOT_FOUND], item.CountryName);
                        result.IsValid = false;
                    }
                    else
                    {
                        result.CountryId = country.Id;
                    }
                }
                if (string.IsNullOrEmpty(item.ModeOfTransport))
                {
                    result.ModeOfTransportError = stringLocalizer[CatalogueLanguageSub.MSG_PLACE_PORTINDEX_MODE_EMPTY];
                    result.IsValid = false;
                }
                else
                {
                    if (DataEnums.ModeOfTransportData.Any(x => x.Id.ToUpper() == item.ModeOfTransport.ToUpper()))
                    {
                        result.ModeOfTransport = item.ModeOfTransport.ToUpper();
                    }
                    else
                    {
                        result.ModeOfTransportError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PLACE_PORTINDEX_MODE_NOT_FOUND], item.ModeOfTransport);
                        result.IsValid = false;
                    }
                }
                if (!string.IsNullOrEmpty(item.AreaName))
                {
                    var area = areas.FirstOrDefault(i => i.NameEn.ToLower() == item.AreaName.ToLower());
                    if (area == null)
                    {
                        result.AreaNameError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PLACE_PORTINDEX_AREA_NOT_FOUND], item.CountryName);
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

        private List<CatPlaceImportModel> CheckWarehouseValidImport(List<CatPlaceImportModel> list, string placeTypeName)
        {
            var countries = countryRepository.Get().ToList();
            var provinces = DataContext.Get(x => x.PlaceTypeId == PlaceTypeEx.GetPlaceType(CatPlaceTypeEnum.Province)).ToList();
            var districts = DataContext.Get(x => x.PlaceTypeId == PlaceTypeEx.GetPlaceType(CatPlaceTypeEnum.District)).ToList();
            var warehouses = DataContext.Get(x => x.PlaceTypeId == placeTypeName).ToList();
            var results = new List<CatPlaceImportModel>();
            foreach (var item in list)
            {
                var result = CheckCatplaceValidImport(warehouses, results, item);
                result.PlaceTypeId = placeTypeName;
                if (string.IsNullOrEmpty(item.Address))
                {
                    result.AddressError = stringLocalizer[CatalogueLanguageSub.MSG_PLACE_ADDRESS_EMPTY];
                    result.IsValid = false;
                }
                else
                {
                    result.Address = item.Address;
                }
                if (string.IsNullOrEmpty(item.CountryName))
                {
                    result.CountryNameError = stringLocalizer[CatalogueLanguageSub.MSG_PLACE_COUNTRY_NAME_EMPTY];
                    result.IsValid = false;
                }
                else
                {
                    var country = countries.FirstOrDefault(i => i.NameEn.ToLower() == item.CountryName.ToLower());
                    if (country == null)
                    {
                        result.CountryNameError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PLACE_COUNTRY_NOT_FOUND], item.CountryName);
                        result.IsValid = false;
                    }
                    else
                    {
                        result.CountryId = country.Id;
                        if (!string.IsNullOrEmpty(item.ProvinceName))
                        {
                            var province = provinces.FirstOrDefault(i => i.NameEn.ToLower() == item.ProvinceName.ToLower() && (i.CountryId == country.Id || country == null));
                            if (province == null)
                            {
                                result.ProvinceNameError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PLACE_PROVINCE_NOT_FOUND], item.ProvinceName, item.CountryName);
                                result.IsValid = false;
                            }
                            else
                            {
                                result.ProvinceId = province.Id;
                                if (!string.IsNullOrEmpty(item.DistrictName))
                                {
                                    var district = districts.FirstOrDefault(i => i.NameEn.ToLower() == item.DistrictName.ToLower() && (i.ProvinceId == province.Id || province == null));
                                    if (district == null)
                                    {
                                        result.DistrictNameError = string.Format(stringLocalizer[CatalogueLanguageSub.MSG_PLACE_DISTRICT_NOT_FOUND], item.DistrictName, item.ProvinceName);
                                        result.IsValid = false;
                                    }
                                    else
                                    {
                                        result.DistrictId = district.Id;
                                    }
                                }
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
                foreach (var item in data)
                {
                    bool active = string.IsNullOrEmpty(item.Status) || (item.Status.ToLower() == "active");
                    DateTime? inactiveDate = active == false ? (DateTime?)DateTime.Now : null;
                    var catPlace = new CatPlace
                    {
                        Id = Guid.NewGuid(),
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
                        Active = active,
                        InactiveOn = inactiveDate,
                        ModeOfTransport = item.ModeOfTransport,
                        AreaId = item.AreaId,
                        CompanyId = currentUser.CompanyID,
                        OfficeId = currentUser.OfficeID,
                        GroupId = currentUser.GroupId,
                        DepartmentId = currentUser.DepartmentId
                    };
                    DataContext.Add(catPlace, false);
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
        public IQueryable<CatPlaceModel> GetByModeOfTran()
        {
            IQueryable<CatPlace> data = null;
            data = DataContext.Get();
            var results = data?.Select(x => mapper.Map<CatPlaceModel>(x));
            results = results.Where(x => ((x.ModeOfTransport ?? "").IndexOf("INLAND - SEA", StringComparison.OrdinalIgnoreCase) > -1));
            return results;

        }

        public List<vw_catProvince> GetAllProvinces()
        {
            var data = GetProvinces().ToList();
            return data;
        }

        public bool CheckAllowPermissionAction(Guid placeID, PermissionRange range)
        {
            var detail = DataContext.Get(x => x.Id == placeID)?.FirstOrDefault();
            if (detail == null) return false;

            BaseUpdateModel baseModel = new BaseUpdateModel
            {
                UserCreated = detail.UserCreated,
                CompanyId = detail.CompanyId,
                DepartmentId = detail.DepartmentId,
                OfficeId = detail.OfficeId,
                GroupId = detail.GroupId
            };
            int code = PermissionExtention.GetPermissionCommonItem(baseModel, range, currentUser);

            if (code == 403) return false;

            return true;
        }

        private IQueryable<sp_GetCatPlace> QueryCriteria(CatPlaceCriteria criteria)
        {
            string placetype = PlaceTypeEx.GetPlaceType(criteria.PlaceType);
            var list = GetBy(placetype);
            
            if (criteria.All == null)
            {
                list = list.Where(x => ((x.Code ?? "").IndexOf(criteria.Code ?? "", StringComparison.OrdinalIgnoreCase) > -1)
                                    && ((x.NameEn ?? "").IndexOf(criteria.NameEn ?? "", StringComparison.OrdinalIgnoreCase) > -1)
                                    && ((x.NameVn ?? "").IndexOf(criteria.NameVn ?? "", StringComparison.OrdinalIgnoreCase) > -1)
                                    && ((x.CountryNameEN ?? "").IndexOf(criteria.CountryNameEN ?? "", StringComparison.OrdinalIgnoreCase) > -1)
                                    && ((x.CountryNameVN ?? "").IndexOf(criteria.CountryNameVN ?? "", StringComparison.OrdinalIgnoreCase) > -1)
                                    && ((x.DistrictNameEN ?? "").IndexOf(criteria.DistrictNameEN ?? "", StringComparison.OrdinalIgnoreCase) > -1)
                                    && ((x.DistrictNameVN ?? "").IndexOf(criteria.DistrictNameVN ?? "", StringComparison.OrdinalIgnoreCase) > -1)
                                    && (x.CountryID == criteria.CountryId || criteria.CountryId == null)
                                    && (x.ProvinceID == criteria.ProvinceId || criteria.ProvinceId == null)
                                    && (x.DistrictID == criteria.DistrictId || criteria.DistrictId == null)
                                    && ((x.ProvinceNameEN ?? "").IndexOf(criteria.ProvinceNameEN ?? "", StringComparison.OrdinalIgnoreCase) > -1)
                                    && ((x.ProvinceNameVN ?? "").IndexOf(criteria.ProvinceNAmeVN ?? "", StringComparison.OrdinalIgnoreCase) > -1)
                                    && ((x.Address ?? "").IndexOf(criteria.Address ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                                    && ((x.ModeOfTransport ?? "").IndexOf(criteria.ModeOfTransport ?? "", StringComparison.OrdinalIgnoreCase) > -1)
                                    && (x.AreaNameEN ?? "").IndexOf(criteria.AreaNameEN ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                    && (x.AreaNameVN ?? "").IndexOf(criteria.AreaNameVN ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                    && (x.FlightVesselNo ?? "").Contains(criteria.FlightVesselNo ?? "", StringComparison.OrdinalIgnoreCase) == true
                                    && (x.Active == criteria.Active || criteria.Active == null)
                    )?.AsQueryable();
            }
            else
            {
                list = list.Where(x => (
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
                                   || (x.FlightVesselNo ?? "").Contains(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) == true
                                   )
                                   && (x.Active == criteria.Active || criteria.Active == null)
                                   )?.AsQueryable();
            }
            return list;
        }

        public CatPlaceModel GetDetail(Guid id)
        {
            ICurrentUser _user = null;
            CatPlace data = DataContext.First(x => x.Id == id);
            if (data == null)
            {
                return null;
            }

            CatPlaceModel result =  mapper.Map<CatPlaceModel>(data);

            if (data.PlaceTypeId == CatPlaceTypeEnum.Warehouse.ToString())
            {
                _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.catWarehouse);
            }
            if (data.PlaceTypeId == CatPlaceTypeEnum.Port.ToString())
            {
                _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.catPortindex);
            }
            if (data.PlaceTypeId == CatPlaceTypeEnum.Province.ToString() 
                || data.PlaceTypeId == CatPlaceTypeEnum.District.ToString() 
                || data.PlaceTypeId == CatPlaceTypeEnum.Ward.ToString())
            {
                _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.catLocation);
            }

            var permissionRangeWrite = PermissionExtention.GetPermissionRange(currentUser.UserMenuPermission.Write);

            BaseUpdateModel baseModel = new BaseUpdateModel
            {
                UserCreated = result.UserCreated,
                CompanyId = result.CompanyId,
                DepartmentId = result.DepartmentId,
                OfficeId = result.OfficeId,
                GroupId = result.GroupId
            };
            result.Permission = new PermissionAllowBase
            {
                AllowUpdate = PermissionExtention.GetPermissionDetail(permissionRangeWrite, baseModel, currentUser),
            };

            return result;
        }

        public IQueryable<sp_GetCatPlace> QueryExport(CatPlaceCriteria criteria)
        {
            IQueryable<sp_GetCatPlace> data = null;

            if (criteria.PlaceType == CatPlaceTypeEnum.Warehouse)
            {
                ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.catWarehouse);
                var rangeSearch = PermissionExtention.GetPermissionRange(currentUser.UserMenuPermission.List);
                if (rangeSearch == PermissionRange.None)
                {
                    return null;
                }
                data = QueryByPermission(criteria, rangeSearch);
            }
            else if (criteria.PlaceType == CatPlaceTypeEnum.Port)
            {
                ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.catPortindex);
                var rangeSearch = PermissionExtention.GetPermissionRange(currentUser.UserMenuPermission.List);
                if (rangeSearch == PermissionRange.None)
                {
                    return null;
                }
                data = QueryByPermission(criteria, rangeSearch);
            }
            else
            {
                data = Query(criteria);

            }

            return data;
        }
    }
} 
