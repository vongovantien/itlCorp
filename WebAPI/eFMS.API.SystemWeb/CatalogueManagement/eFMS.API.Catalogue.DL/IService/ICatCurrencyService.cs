using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Catalogue.DL.IService
{
    public interface ICatCurrencyService:IRepositoryBase<CatCurrency,CatCurrencyModel>
    {
        List<CatCurrency> Paging(CatCurrrencyCriteria criteria, int pageNumber, int pageSize, out int rowsCount);
        HandleState Update(CatCurrencyModel model);
        HandleState Delete(string id);
    }
}
