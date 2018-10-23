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

namespace eFMS.API.Catalogue.DL.Services
{
    public class CatPlaceService : RepositoryBase<CatPlace, CatPlaceModel>, ICatPlaceService
    {
        public CatPlaceService(IContextBase<CatPlace> repository, IMapper mapper) : base(repository, mapper)
        {
        }

        public List<vw_catProvince> GetProvinces(short? countryId)
        {
            var data = GetProvinces().Where(x => x.CountryID == countryId || countryId == null).ToList();
            return data;
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
                list = list.Skip((page-1)* size).Take(size).ToList();
            }
            return list;
        }

        public List<vw_catPlace> Query(CatPlaceCriteria criteria)
        {
            var list = GetView();
            string placetype = PlaceTypeEx.GetPlaceType(criteria.PlaceType);
            if(criteria.All == null)
            {
                list = list.Where(x => ((x.Code ?? "").IndexOf(criteria.Code ?? "", StringComparison.OrdinalIgnoreCase) >=0)
                                    && ((x.Name_EN ?? "").IndexOf(criteria.NameEn ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                                    && ((x.Name_VN ?? "").IndexOf(criteria.NameVn ?? "", StringComparison.OrdinalIgnoreCase) >=0)
                                    && ((x.CountryNameEN ?? "").IndexOf(criteria.CountryNameEN ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                                    && ((x.CountryNameVN ?? "").IndexOf(criteria.CountryNameVN ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                                    && ((x.DistrictNameEN ?? "").IndexOf(criteria.DistrictNameEN ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                                    && ((x.DistrictNameVN ?? "").IndexOf(criteria.DistrictNameVN ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                                    && ((x.ProvinceNameEN ?? "").IndexOf(criteria.ProvinceNameEN ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                                    && ((x.ProvinceNameVN ?? "").IndexOf(criteria.ProvinceNAmeVN ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                                    && ((x.Address ?? "").IndexOf(criteria.Address ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                                    && ((x.PlaceTypeID ?? "").IndexOf(placetype ?? "", StringComparison.OrdinalIgnoreCase) >= 0)
                    ).ToList();
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
                                   ).ToList();
            }
            return list;
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
    }
}
