using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Catalogue.DL.Models.Criteria;
using eFMS.API.Catalogue.DL.ViewModels;
using eFMS.API.Catalogue.Service.Models;
using eFMS.API.Catalogue.Service.ViewModels;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Catalogue.DL.IService
{
    public interface ICatCurrencyExchangeService : IRepositoryBase<CatCurrencyExchange, CatCurrencyExchangeModel>
    {
        List<CatCurrencyExchangeHistory> Paging(CatCurrencyExchangeCriteria criteria, int page, int size, out int rowsCount);
        List<vw_catCurrencyExchange> Query(CatCurrencyExchangeCriteria criteria);
        CurrencyExchangeNewestViewModel GetCurrencyExchangeNewest(string currencyToId);
        CurrencyExchangeNewestViewModel GetExchangeRates(DateTime date, string localCurrency, string fromCurrency);
        vw_catCurrencyExchangeNewest ConvertRate(DateTime date, string localCurrency, string fromCurrency);
        HandleState UpdateRate(CatCurrencyExchangeEditModel model);
        object GetCurrency();
        HandleState RemoveExchangeCurrency(string currencyFrom, string currentUser);
    }
}
