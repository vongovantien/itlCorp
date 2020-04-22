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
    public interface ICatCommodityGroupService : IRepositoryBaseCache<CatCommodityGroup, CatCommodityGroupModel>
    {
        IQueryable<CatCommodityGroupModel> Query(CatCommodityGroupCriteria criteria);
        IQueryable<CatCommodityGroupModel> Paging(CatCommodityGroupCriteria criteria, int page, int size, out int rowsCount);
        List<CatCommodityGroupViewModel> GetByLanguage();
        List<CommodityGroupImportModel> CheckValidImport(List<CommodityGroupImportModel> list);
        HandleState Import(List<CommodityGroupImportModel> data);
    }
}
