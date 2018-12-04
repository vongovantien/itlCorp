using eFMS.API.Log.DL.Models;
using eFMS.API.Log.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace eFMS.API.Log.DL.IService
{
    public interface ICatCurrencyService
    {
        IEnumerable<CatCurrency> Get();
        List<LogModel> Paging(string query, int page, int size, out long rowsCount);
        CatCurrency Get(Guid id);
        bool Remove(Guid id);
    }
}
