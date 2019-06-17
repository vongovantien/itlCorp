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
    public interface ICatCountryService : IRepositoryBase<CatCountry, CatCountryModel>
    {
        List<CatCountryViewModel> GetByLanguage();
        List<CatCountry> GetCountries(CatCountryCriteria criteria, int page, int size, out int rowsCount);
        IQueryable<CatCountry> Query(CatCountryCriteria criteria);
        List<CatCountryImportModel> CheckValidImport(List<CatCountryImportModel> list);
        HandleState Import(List<CatCountryImportModel> data);
        HandleState Update(CatCountryModel model);
        HandleState Delete(short id);
    }
}
