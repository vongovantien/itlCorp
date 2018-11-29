using eFMS.API.Log.Service.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace eFMS.API.Log.DL.IService
{
    public interface ICatCurrencyService
    {
        Task<IEnumerable<CatCurrency>> GetAll();
        IEnumerable<CatCurrency> Get();
        Task<CatCurrency> GetAsync(Guid id);
        Task Add(CatCurrency item);
        Task<bool> Remove(Guid id);
        Task<bool> Update(CatCurrency item);
    }
}
