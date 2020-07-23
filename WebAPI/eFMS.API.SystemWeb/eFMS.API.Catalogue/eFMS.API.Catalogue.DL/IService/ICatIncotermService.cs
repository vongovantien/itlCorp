using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Linq;
using eFMS.API.Catalogue.Models;

namespace eFMS.API.Catalogue.DL.IService
{
    public interface ICatIncotermService : IRepositoryBase<CatIncoterm, CatIncotermModel>
    {
        IQueryable<CatIncoterm> Query(CatIncotermCriteria criteria);
        IQueryable<CatIncoterm> Paging(CatIncotermCriteria criteria, int page, int size, out int rowsCount);
        //HandleState Add(CatChartOfAccounts model);
        HandleState Update(CatIncotermEditModel model);
        HandleState AddNew(CatIncotermEditModel model);
        HandleState Delete(Guid Id);
        CatChartOfAccountsModel GetDetail(Guid id);
        bool CheckAllowDelete(Guid id);
        bool CheckAllowViewDetail(Guid id);
    }
}
