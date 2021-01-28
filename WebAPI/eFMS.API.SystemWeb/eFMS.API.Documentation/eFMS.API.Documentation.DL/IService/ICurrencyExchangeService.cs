using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Documentation.DL.IService
{
    public interface ICurrencyExchangeService : IRepositoryBase<CatCurrencyExchange, CatCurrencyExchangeModel>
    {
        decimal GetRateCurrencyExchange(IQueryable<CatCurrencyExchange> currencyExchange, string currencyFrom, string currencyTo);

        decimal CurrencyExchangeRateConvert(decimal? finalExchangeRate, DateTime? exchangeDate, string currencyFrom, string currencyTo);

        decimal CalculatorAmount(decimal? amount, decimal? finalExchangeRate, DateTime? exchangeDate, string currencyFrom, string currencyTo, int? roundCurr);
    }
}
