using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.DL.ViewModels;
using eFMS.API.Catalogue.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Catalogue.DL.IService
{
    public interface ICatDistrictService : IRepositoryBaseCache<CatDistrict, CatDistrictModel>
    {
        List<CatDistrictViewModel> GetByLanguage();
        IQueryable<CatDistrictModel> GetDistricts(CatDistrictCriteria criteria, int page, int size, out int rowsCount);
        IQueryable<CatDistrictModel> Query(CatDistrictCriteria criteria);
        List<CatDistrictModel> CheckValidImport(List<CatDistrictModel> list);
        HandleState Import(List<CatDistrictModel> data);
        HandleState Delete(Guid id);
        HandleState Update(CatDistrictModel model);
        List<CatDistrict> GetDistrictsByCity(Guid? cityId);
    }
}
