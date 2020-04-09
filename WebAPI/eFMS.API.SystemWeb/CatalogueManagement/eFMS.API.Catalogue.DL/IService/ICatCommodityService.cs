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
    public interface ICatCommodityService : IRepositoryBaseCache<CatCommodity, CatCommodityModel>
    {
        IQueryable<CatCommodityModel> GetAll();
        IQueryable<CatCommodityModel> Query(CatCommodityCriteria criteria);
        List<CatCommodityModel> Paging(CatCommodityCriteria criteria, int page, int size, out int rowsCount);
        List<CommodityImportModel> CheckValidImport(List<CommodityImportModel> list);
        HandleState Import(List<CommodityImportModel> data);
        HandleState Update(CatCommodityModel model);
        HandleState Delete(int id);
    }
}
