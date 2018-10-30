using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.DL.ViewModels;
using eFMS.API.Catalogue.Service.Models;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Catalogue.DL.IService
{
    public interface ICatCommodityService : IRepositoryBase<CatCommodity, CatCommodityModel>
    {
        List<CatCommodityViewModel> Query(CatCommodityCriteria criteria);
        List<CatCommodityViewModel> Paging(CatCommodityCriteria criteria, int page, int size, out int rowsCount);
    }
}
