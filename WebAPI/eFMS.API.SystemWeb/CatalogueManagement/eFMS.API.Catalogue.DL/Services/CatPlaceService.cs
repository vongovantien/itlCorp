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

        public List<WarehouseImportModel> CheckValidImport(List<WarehouseImportModel> list, CatPlaceTypeEnum placeType)
        {
            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            var countries = dc.CatCountry;
            var provinces = dc.CatPlace.Where(x => x.PlaceTypeId == PlaceTypeEx.GetPlaceType(CatPlaceTypeEnum.Province));
            var districts = dc.CatPlace.Where(x => x.PlaceTypeId == PlaceTypeEx.GetPlaceType(CatPlaceTypeEnum.District));
            var warehouses = dc.CatPlace.Where(x => x.PlaceTypeId == PlaceTypeEx.GetPlaceType(CatPlaceTypeEnum.Warehouse));
            list.ForEach(x => {
                var country = countries.FirstOrDefault(i => i.NameEn.IndexOf(x.CountryName) >= 0);
                //var province = provinces.FirstOrDefault(i => i.NameEn.IndexOf(x.ProvinceName) >= 0 && (i.CountryId == country.Id || country == null));
                //var district = districts.FirstOrDefault(i => i.NameEn.IndexOf(x.DistrictName) >= 0 && (i.ProvinceId == province.Id || province == null));
                var warehouse = warehouses.FirstOrDefault(i => i.Code.IndexOf(x.Code) >= 0);
                if(warehouse != null)
                {
                    x.InvalidMessage = string.Format("Code '{0}' is existed!", x.Code);
                }
                else
                {
                    if (country == null)
                    {
                        x.InvalidMessage = string.Format("Country '{0}' is not found!", x.CountryName);
                    }
                    else
                    {
                        x.CountryId = country.Id;
                        var province = provinces.FirstOrDefault(i => i.NameEn.IndexOf(x.ProvinceName) >= 0 && (i.CountryId == country.Id || country == null));
                        if (province == null)
                        {
                            x.InvalidMessage = string.Format("Province '{0}' is not found!", x.ProvinceName);
                        }
                        else
                        {
                            x.ProvinceId = province.Id;
                            var district = districts.FirstOrDefault(i => i.NameEn.IndexOf(x.DistrictName) >= 0 && (i.ProvinceId == province.Id || province == null));
                            if (district == null)
                            {
                                x.InvalidMessage = string.Format("District '{0}' is not found!", x.DistrictName);
                            }
                            else
                            {
                                x.DistrictId = district.Id;
                            }
                        }
                    }
                    x.PlaceTypeId = PlaceTypeEx.GetPlaceType(placeType);
                    x.Status = x.Inactive == false ? DataEnums.EnInActive : DataEnums.EnActive;
                }
            });
            return list;
        }

        public HandleState Import(List<WarehouseImportModel> data)
        {
            try
            {
                eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
                foreach (var item in data)
                {
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
                        PlaceTypeId = item.PlaceTypeId
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
