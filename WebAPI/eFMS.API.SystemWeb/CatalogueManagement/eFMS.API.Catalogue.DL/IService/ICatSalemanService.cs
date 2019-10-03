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
    public interface ICatSaleManService : IRepositoryBase<CatSaleMan, CatSaleManModel>
    {
        IQueryable<CatSaleMan> GetSaleMan();
        List<CatSaleManViewModel> Query(CatSaleManCriteria criteria);
        List<CatSaleManViewModel> Paging(CatSaleManCriteria criteria, int page, int size, out int rowsCount);
        HandleState Delete(Guid id);
        HandleState Update(CatSaleManModel model);
        List<CatSaleManModel> GetBy(string partnerId);
    }
}
