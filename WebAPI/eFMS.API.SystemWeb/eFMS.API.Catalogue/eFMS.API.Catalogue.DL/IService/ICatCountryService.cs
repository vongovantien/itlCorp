using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.DL.ViewModels;
using eFMS.API.Catalogue.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Catalogue.DL.IService
{
    public interface ICatCountryService : IRepositoryBaseCache<CatCountry, CatCountryModel>
    {
        List<CatCountryViewModel> GetByLanguage();
        IQueryable<CatCountryModel> GetCountries(CatCountryCriteria criteria, int page, int size, out int rowsCount);
        IQueryable<CatCountryModel> Query(CatCountryCriteria criteria);
        List<CatCountryImportModel> CheckValidImport(List<CatCountryImportModel> list);
        HandleState Import(List<CatCountryImportModel> data);
        HandleState Delete(short id);
        HandleState Update(CatCountryModel model);
    }
}
