using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Catalogue.DL.IService
{
    public interface ICatChartOfAccountsService : IRepositoryBaseCache<CatChartOfAccounts, CatChartOfAccountsModel>
    {
        IQueryable<CatChartOfAccounts> Query(CatChartOfAccountsCriteria criteria);
        IQueryable<CatChartOfAccounts> Paging(CatChartOfAccountsCriteria criteria, int page, int size, out int rowsCount);

        //HandleState Add(CatChartOfAccounts model);
        HandleState Update(CatChartOfAccounts model);
        HandleState Delete(Guid idAcc);
        CatChartOfAccountsModel GetDetail(Guid id);
        bool CheckAllowDelete(Guid id);
        bool CheckAllowViewDetail(Guid id);



    }
}
