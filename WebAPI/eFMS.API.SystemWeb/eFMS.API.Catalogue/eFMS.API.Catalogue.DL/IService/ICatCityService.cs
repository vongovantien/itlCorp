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
    public interface ICatCityService : IRepositoryBaseCache<CatCity, CatCityModel>
    {
        List<CatCityViewModel> GetByLanguage();
        IQueryable<CatCityModel> GetCities(CatCityCriteria criteria, int page, int size, out int rowsCount);
        IQueryable<CatCityModel> Query(CatCityCriteria criteria);
        List<CatCityModel> CheckValidImport(List<CatCityModel> list);
        HandleState Import(List<CatCityModel> data);
        HandleState Delete(Guid id);
        HandleState Update(CatCityModel model);
        List<CatCity> GetCitiesByCountry(short? countryId);
    }
}
