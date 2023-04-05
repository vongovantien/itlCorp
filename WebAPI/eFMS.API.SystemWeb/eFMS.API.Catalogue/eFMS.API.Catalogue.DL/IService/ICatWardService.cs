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
    public interface ICatWardService : IRepositoryBaseCache<CatWard, CatWardModel>
    {
        List<CatWardViewModel> GetByLanguage();
        IQueryable<CatWardModel> GetWards(CatWardCriteria criteria, int page, int size, out int rowsCount);
        IQueryable<CatWardModel> Query(CatWardCriteria criteria);
        List<CatWardModel> CheckValidImport(List<CatWardModel> list);
        HandleState Import(List<CatWardModel> data);
        HandleState Delete(short id);
        HandleState Update(CatWardModel model);
        List<CatWard> GetWardsByDistrict(string code);
    }
}
