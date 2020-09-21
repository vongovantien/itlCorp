using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Linq;
using eFMS.API.Catalogue.Models;
using eFMS.API.Provider.Services.IService;

namespace eFMS.API.Catalogue.DL.IService
{
    public interface ICatIncotermService : IRepositoryBase<CatIncoterm, CatIncotermModel>, IPermissionBaseService<CatIncoterm, CatIncotermModel>
    {
        IQueryable<CatIncotermModel> Query(CatIncotermCriteria criteria);
        IQueryable<CatIncotermModel> Paging(CatIncotermCriteria criteria, int page, int size, out int rowsCount);
        HandleState Update(CatIncotermEditModel model);
        HandleState AddNew(CatIncotermEditModel model);
        HandleState Delete(Guid Id);
        CatIncotermEditModel GetDetail(Guid id);
        IQueryable<CatIncotermModel> QueryExport(CatIncotermCriteria criteria);
       
    }
}
