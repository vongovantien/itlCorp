using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System.Linq;

namespace eFMS.API.Catalogue.DL.IService
{
    public interface ICatCurrencyService: IRepositoryBaseCache<CatCurrency,CatCurrencyModel>
    {
        IQueryable<CatCurrencyModel> GetAll();
        IQueryable<CatCurrencyModel> Paging(CatCurrrencyCriteria criteria, int pageNumber, int pageSize, out int rowsCount);
        HandleState Update(CatCurrencyModel model);
        HandleState Delete(string id);

        IQueryable<CatCurrencyModel> Query(CatCurrrencyCriteria criteria);


    }
}
