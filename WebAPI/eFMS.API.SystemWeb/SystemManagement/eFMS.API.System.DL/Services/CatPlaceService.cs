using AutoMapper;
using eFMS.API.Common.Globals;
using eFMS.API.Catalog.DL.IService;
using eFMS.API.Catalog.DL.Models;
using eFMS.API.Catalog.DL.Models.Criteria;
using eFMS.API.Catalog.Service.Models;
using eFMS.API.Catalog.Service.ViewModels;
using ITL.NetCore.Connection;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Catalog.DL.Services
{
    public class CatPlaceService : RepositoryBase<CatPlace, CatPlaceModel>, ICatPlaceService
    {
        public CatPlaceService(IContextBase<CatPlace> repository, IMapper mapper) : base(repository, mapper)
        {
            SetUnique(new string[] { "Code", "NameVn", "NameEn" });
        }

        public List<vw_catPlace> Paging(CatPlaceCriteria criteria, int page, int size, out int rowsCount)
        {
            var list = Query(criteria);
            rowsCount = list.Count;
            if (size > 1)
            {
                if (page < 1)
                {
                    page = 1;
                }
                list = list.Skip(page).Take(size).ToList();
            }
            return list;
        }

        public List<vw_catPlace> Query(CatPlaceCriteria criteria)
        {
            var list = GetView();
            string placetype = GetPlaceType(criteria.PlaceType);
            list = list.Where(x => (x.Code ?? "").Contains(criteria.Code ?? "")
                                && (x.Name_EN ?? "").Contains(criteria.NameEn ?? "")
                                && (x.Name_VN ?? "").Contains(criteria.NameVn ?? "")
                                && (x.CountryNameEN ?? "").Contains(criteria.CountryNameEN ?? "")
                                && (x.CountryNameVN ?? "").Contains(criteria.CountryNameVN ?? "")
                                && (x.DistrictNameEN?? "").Contains(criteria.DistrictNameEN ?? "")
                                && (x.DistrictNameVN ?? "").Contains(criteria.DistrictNameVN ?? "")
                                && (x.ProvinceNameEN ?? "").Contains(criteria.ProvinceNameEN ?? "")
                                && (x.ProvinceNameVN ?? "").Contains(criteria.ProvinceNAmeVN ?? "")
                                && (x.Address ?? "").Contains(criteria.Address ?? "")
                                && (x.PlaceTypeID ?? "").Contains(placetype ?? "")
                ).ToList();
            return list;
        }

        private string GetPlaceType(CatPlaceTypeEnum placeType)
        {
            string result = "";
            switch (placeType)
            {
                case CatPlaceTypeEnum.BorderGate:
                    result = "BorderGate";
                    break;
                case CatPlaceTypeEnum.Branch:
                    result = "Branch";
                    break;
                case CatPlaceTypeEnum.Depot:
                    result = "Depot";
                    break;
                case CatPlaceTypeEnum.District:
                    result = "District";
                    break;
                case CatPlaceTypeEnum.Hub:
                    result = "Hub";
                    break;
                case CatPlaceTypeEnum.IndustrialZone:
                    result = "IndustrialZone";
                    break;
                case CatPlaceTypeEnum.Other:
                    result = "Other";
                    break;
                case CatPlaceTypeEnum.Port:
                    result = "Port";
                    break;
                case CatPlaceTypeEnum.Province:
                    result = "Province";
                    break;
                case CatPlaceTypeEnum.Station:
                    result = "Station";
                    break;
                case CatPlaceTypeEnum.Ward:
                    result = "Ward";
                    break;
                case CatPlaceTypeEnum.Warehouse:
                    result = "Warehouse";
                    break;
                default:
                    break;
            }
            return result;
        }

        private List<vw_catPlace> GetView()
        {
            List<vw_catPlace> lvCatPlace = ((eFMSDataContext)DataContext.DC).GetViewData<vw_catPlace>();
            return lvCatPlace;
        }
    }
}
