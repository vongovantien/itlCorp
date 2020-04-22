using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.DL.ViewModels;
using eFMS.API.Catalogue.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Catalogue.DL.IService
{
    public interface ICatSaleManService : IRepositoryBase<CatSaleman, CatSaleManModel>
    {
        IQueryable<CatSaleman> GetSaleMan();
        IQueryable<CatSaleManViewModel> Query(CatSalemanCriteria criteria);

        List<CatSaleManViewModel> Paging(CatSalemanCriteria criteria, int page, int size, out int rowsCount);
        HandleState Delete(Guid id);
        HandleState Update(CatSaleManModel model);
        List<CatSaleManModel> GetBy(string partnerId);
        Guid? GetSalemanIdByPartnerId(string partnerId);



    }
}
