using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Service.Models;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Catalogue.DL.IService
{
    public interface ICatChartOfAccountsService : IRepositoryBaseCache<CatChartOfAccounts, CatChartOfAccountsModel>
    {
        IQueryable<CatChartOfAccountsModel> Query(CatChartOfAccountsCriteria criteria);
        IQueryable<CatChartOfAccountsModel> Paging(CatChartOfAccountsCriteria criteria, int page, int size, out int rowsCount);
    }
}
