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
using System.Text;
using eFMS.API.Catalogue.DL.Common;
using System.Linq.Expressions;
using eFMS.API.Catalogue.DL.ViewModels;
using System.Threading;
using System.Globalization;
using ITL.NetCore.Common;
using eFMS.API.Catalogue.Service.Helpers;

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatPlaceService : RepositoryBase<CatPlace, CatPlaceModel>, ICatPlaceService
    {
        public CatPlaceService(IContextBase<CatPlace> repository, IMapper mapper) : base(repository, mapper)
        {
            SetChildren<CatCountry>("Id", "CountryId");
            SetChildren<CatPlace>("Id", "ProvinceId");
            SetChildren<CatPlace>("Id", "DistrictId");
        }

        public List<vw_catProvince> GetProvinces(short? countryId)
        {
            var data = GetProvinces().Where(x => x.CountryID == countryId || countryId == null).ToList();
            return data;
        }

        public List<CatPlaceViewModel> Paging(CatPlaceCriteria criteria, int page, int size, out int rowsCount)
        {
            var list = Query(criteria);
            rowsCount = list.Count;
            if (size > 1)
            {
                if (page < 1)
                {
                    page = 1;
                }
                list = list.Skip((page-1)* size).Take(size).ToList();
            }
            return GetCulturalData(list);
        }

        public List<vw_catPlace> Query(CatPlaceCriteria criteria)
        {
            var list = GetView();
            string placetype = PlaceTypeEx.GetPlaceType(criteria.PlaceType);
            if (criteria.All == null)
            {
                list = list.Where(x => ((x.Code ?? "").IndexOf(criteria.Code ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                                    && ((x.Name_EN ?? "").IndexOf(criteria.NameEn ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                                    && ((x.Name_VN ?? "").IndexOf(criteria.NameVn ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                                    && ((x.CountryNameEN ?? "").IndexOf(criteria.CountryNameEN ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                                    && ((x.CountryNameVN ?? "").IndexOf(criteria.CountryNameVN ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                                    && ((x.DistrictNameEN ?? "").IndexOf(criteria.DistrictNameEN ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                                    && ((x.DistrictNameVN ?? "").IndexOf(criteria.DistrictNameVN ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                                    && (x.CountryID  == criteria.CountryId || criteria.CountryId==null)
                                    && (x.ProvinceID == criteria.ProvinceId || criteria.ProvinceId == null)
                                    && (x.DistrictID == criteria.DistrictId || criteria.DistrictId == null)
                                    && ((x.ProvinceNameEN ?? "").IndexOf(criteria.ProvinceNameEN ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                                    && ((x.ProvinceNameVN ?? "").IndexOf(criteria.ProvinceNAmeVN ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                                    && ((x.Address ?? "").IndexOf(criteria.Address ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                                    && ((x.PlaceTypeID ?? "").IndexOf(placetype ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                    ).OrderBy(x => x.Code).ToList();
            }
            else
            {
                list = list.Where(x => (
                                      ((x.Code ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                                   || ((x.Name_EN ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                                   || ((x.Name_VN ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                                   || ((x.CountryNameEN ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                                   || ((x.CountryNameVN ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                                   || ((x.DistrictNameEN ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                                   || ((x.DistrictNameVN ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                                   || ((x.ProvinceNameEN ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                                   || ((x.ProvinceNameVN ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                                   || ((x.Address ?? "").IndexOf(criteria.All ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                                   )
                                   && ((x.PlaceTypeID ?? "").IndexOf(placetype ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                                   ).OrderBy(x => x.Code).ToList();
            }
            return list;
        }

        private List<CatPlaceViewModel> GetCulturalData(List<vw_catPlace> list)
        {
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            if (currentCulture.Name == "vi-VN")
            {
                 return list.Select(x => new CatPlaceViewModel
                {
                    ID = x.ID,
                    Code = x.Code,
                    NameEN = x.Name_EN,
                    NameVN = x.Name_VN,
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
                    Inactive = x.Inactive,
                    InactiveOn = x.InactiveOn,
                    CountryName = x.CountryNameVN,
                    AreaName = x.AreaNameVN,
                    LocalAreaName = x.LocalAreaNameVN
                }).ToList();
            }
            else
            {
                return list.Select(x => new CatPlaceViewModel
                {
                    ID = x.ID,
                    Code = x.Code,
                    NameEN = x.Name_EN,
                    NameVN = x.Name_VN,
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
                    Inactive = x.Inactive,
                    InactiveOn = x.InactiveOn,
                    CountryName = x.CountryNameEN,
                    AreaName = x.AreaNameEN,
                    LocalAreaName = x.LocalAreaNameEN
                }).ToList();
            }
        }

        private List<vw_catPlace> GetView()
        {
            List<vw_catPlace> lvCatPlace = ((eFMSDataContext)DataContext.DC).GetViewData<vw_catPlace>();
            return lvCatPlace;
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
            var districts = dc.CatPlace.Where(x => x.PlaceTypeId == placeTypeName).ToList();
            var results = new List<CatPlaceImportModel>();
            foreach (var item in list)
            {
                var result = CheckCatplaceValidImport(districts, results, item);
                result.PlaceTypeId = placeTypeName;
                if (string.IsNullOrEmpty(item.CountryName))
                {
                    result.CountryName = string.Format("Country name is not allow empty");
                    result.ProvinceName = string.Format("Country name is not allow empty|wrong");
                    result.DistrictName = string.Format("Country name is not allow empty|wrong");

                    result.IsValid = false;
                }
                else
                {
                    var country = countries.FirstOrDefault(i => (i.NameEn ?? "").IndexOf(item.CountryName ?? "", StringComparison.OrdinalIgnoreCase) >= 0);

                    if (country == null)
                    {
                        result.CountryName = string.Format("Country '{0}' is not found!|wrong", item.CountryName);
                        result.ProvinceName = string.Format("Country '{0}' is not found. Please check country '{0}'!|wrong", item.CountryName);
                        result.DistrictName = string.Format("Country '{0}' is not found. Please check country '{0}'!|wrong", item.CountryName);
                        result.IsValid = false;
                    }
                    else
                    {
                        result.CountryId = country.Id;
                        var province = provinces.FirstOrDefault(i => (i.NameEn ?? "").IndexOf(item.ProvinceName??"", StringComparison.OrdinalIgnoreCase) >= 0 && (i.CountryId == country.Id || country == null));
                        if (string.IsNullOrEmpty(item.ProvinceName))
                        {
                            result.ProvinceName = string.Format("Province name is not allow empty!|wrong");
                            result.DistrictName = string.Format("Province name is not allow empty!|wrong|wrong");
                            result.IsValid = false;
                        }
                        else if (province == null)
                        {
                            result.ProvinceName = string.Format("Province name '{0}' is not found!|wrong", item.ProvinceName);
                            result.IsValid = false;
                        }
                        else
                        {
                            result.ProvinceId = province.Id;
                            if (string.IsNullOrEmpty(item.DistrictName))
                            {
                                result.DistrictName = string.Format("District name is not allow empty!|wrong");
                                result.IsValid = false;
                            }
                            else
                            {
                                var district = districts.FirstOrDefault(i => (i.NameEn ?? "").IndexOf(item.DistrictName??"") >= 0 && (i.ProvinceId == province.Id || province == null));
                                if(district == null)
                                {
                                    result.DistrictName = string.Format("District name is not allow empty!|wrong");
                                    result.IsValid = false;
                                }
                            }
                        }
                    }
                }
                results.Add(result);
            }
            return results;
        }

        private List<CatPlaceImportModel> CheckDistrictValidImport(List<CatPlaceImportModel> list, eFMSDataContext dc, string placeTypeName)
        {
            var countries = dc.CatCountry.ToList();
            var provinces = dc.CatPlace.Where(x => x.PlaceTypeId == PlaceTypeEx.GetPlaceType(CatPlaceTypeEnum.Province)).ToList();
            var districts = dc.CatPlace.Where(x => x.PlaceTypeId == placeTypeName).ToList();
            list.ForEach(item =>
            {
                item.PlaceTypeId = placeTypeName;
                item = CheckCatplaceValidImport(districts, list, item);
                if (string.IsNullOrEmpty(item.CountryName))
                {
                    item.CountryName = string.Format("Country name is not allow empty");
                    item.ProvinceName = string.Format("Country name is not allow empty|wrong");
                    item.IsValid = false;
                }
                else
                {
                    var country = countries.FirstOrDefault(i => i.NameEn.IndexOf(item.CountryName) >= 0);
                    if (country == null)
                    {
                        item.CountryName = string.Format("Country '{0}' is not found!|wrong", item.CountryName);
                        item.ProvinceName = string.Format("Country '{0}' is not found. Please check country '{0}'!|wrong", item.CountryName);
                        item.IsValid = false;
                    }
                    else
                    {
                        item.CountryId = country.Id;
                        var province = provinces.FirstOrDefault(i => i.NameEn.IndexOf(item.ProvinceName) >= 0 && (i.CountryId == country.Id || country == null));
                        if (string.IsNullOrEmpty(item.ProvinceName))
                        {
                            item.ProvinceName = string.Format("Province name is not allow empty!|wrong");
                            item.IsValid = false;
                        }
                        else if (province == null)
                        {
                            item.ProvinceName = string.Format("Province name '{0}' is not found!|wrong", item.ProvinceName);
                            item.IsValid = false;
                        }
                        else
                        {
                            item.ProvinceId = province.Id;
                            var district = districts.FirstOrDefault(i => i.NameEn.IndexOf(item.DistrictName) >= 0 && (i.ProvinceId == province.Id || province == null));
                            if (string.IsNullOrEmpty(item.DistrictName))
                            {
                                item.DistrictName = string.Format("District name is not allow empty!|wrong");
                                item.IsValid = false;
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
                    result.CountryName = string.Format("Country name is not allow empty");
                    result.IsValid = false;
                }
                else
                {
                    var country = countries.FirstOrDefault(i => i.NameEn.IndexOf(item.CountryName) >= 0);
                    if (country == null)
                    {
                        result.CountryName = string.Format("Country '{0}' is not found!|wrong", item.CountryName);
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
                item.Code = string.Format("Code is not allow empty!|wrong");
                item.IsValid = false;
            }
            else if (newList.Any(x => (x.Code ?? "").IndexOf(item.Code ?? "", StringComparison.OrdinalIgnoreCase) >=0))
            {
                item.Code = string.Format("Code {0} is existed!|wrong", item.Code);
                item.IsValid = false;
            }
            else
            {
                if(places.Any(i => (i.Code ?? "").IndexOf(item.Code ?? "", StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    item.Code = string.Format("Code '{0}' has been existed!|wrong", item.Code);
                    item.IsValid = false;
                }
            }
            if (string.IsNullOrEmpty(item.NameEn))
            {
                item.NameEn = string.Format("NameEn is not allow empty!|wrong");
                item.IsValid = false;
            }
            if (string.IsNullOrEmpty(item.NameVn))
            {
                item.NameVn = string.Format("NameVn is not allow empty!|wrong");
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
                if (string.IsNullOrEmpty(item.Address))
                {
                    result.Address = string.Format("Address is not allow empty!|wrong");
                    result.IsValid = false;
                }
                if (string.IsNullOrEmpty(item.CountryName))
                {
                    result.CountryName = string.Format("Country name is not allow empty!|wrong");
                    result.IsValid = false;
                }
                else
                {
                    var country = countries.FirstOrDefault(i => i.NameEn.IndexOf(item.CountryName, StringComparison.OrdinalIgnoreCase) >= 0);
                    if (country == null)
                    {
                        result.CountryName = string.Format("Country '{0}' is not found!|wrong", item.CountryName);
                        result.IsValid = false;
                    }
                    else
                    {
                        result.CountryId = country.Id;
                    }
                }
                if (string.IsNullOrEmpty(item.ModeOfTransport))
                {
                    result.ModeOfTransport = string.Format("Mode of transport is not allow empty!|wrong");
                }
                if (!string.IsNullOrEmpty(item.AreaName))
                {
                    var area = areas.FirstOrDefault(i => i.NameEn.IndexOf(item.AreaName, StringComparison.OrdinalIgnoreCase) >= 0);
                    if (area == null)
                    {
                        result.CountryName = string.Format("Area '{0}' is not found!|wrong", item.CountryName);
                        result.IsValid = false;
                    }
                    else
                    {
                        result.AreaId = area.Id;
                    }
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
                var country = countries.FirstOrDefault(i => (i.NameEn ?? "").IndexOf(item.CountryName ?? "", StringComparison.OrdinalIgnoreCase) >= 0);

                if (country == null)
                {
                    result.CountryName = string.Format("Country '{0}' is not found!|wrong", item.CountryName);
                    result.ProvinceName = string.Format("Country '{0}' is not found. Please check country '{0}'!|wrong", item.CountryName);
                    result.DistrictName = string.Format("Country '{0}' is not found. Please check country '{0}'!|wrong", item.CountryName);
                    result.IsValid = false;
                }
                else
                {
                    result.CountryId = country.Id;
                    var province = provinces.FirstOrDefault(i => (i.NameEn ?? "").IndexOf(item.ProvinceName ?? "", StringComparison.OrdinalIgnoreCase) >= 0 && (i.CountryId == country.Id || country == null));
                    if (string.IsNullOrEmpty(item.ProvinceName))
                    {
                        result.ProvinceName = string.Format("Province name is not allow empty!|wrong");
                        result.DistrictName = string.Format("Province name is not allow empty!|wrong|wrong");
                        result.IsValid = false;
                    }
                    else if (province == null)
                    {
                        result.ProvinceName = string.Format("Province name '{0}' is not found!|wrong", item.ProvinceName);
                        result.IsValid = false;
                    }
                    else
                    {
                        result.ProvinceId = province.Id;
                        if (string.IsNullOrEmpty(item.DistrictName))
                        {
                            result.DistrictName = string.Format("District name is not allow empty!|wrong");
                            result.IsValid = false;
                        }
                        else
                        {
                            var district = districts.FirstOrDefault(i => (i.NameEn ?? "").IndexOf(item.DistrictName ?? "") >= 0 && (i.ProvinceId == province.Id || province == null));
                            if (district == null)
                            {
                                result.DistrictName = string.Format("District name is not allow empty!|wrong");
                                result.IsValid = false;
                            }
                        }
                    }
                }
                result.PlaceTypeId = placeTypeName;
                result.Status = DataEnums.EnActive;
                result.Status = item.Inactive == false ? DataEnums.EnInActive : DataEnums.EnActive;
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
                    DateTime? inactive = null;
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
                        UserCreated = ChangeTrackerHelper.currentUser,
                        PlaceTypeId = item.PlaceTypeId,
                        Inactive = (item.Status ?? "").Contains("active"),
                        InactiveOn = item.Status != null ? DateTime.Now : inactive
                    };
                    dc.CatPlace.Add(catPlace);
                }
                dc.SaveChanges();
                return new HandleState();
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }
    }
}
