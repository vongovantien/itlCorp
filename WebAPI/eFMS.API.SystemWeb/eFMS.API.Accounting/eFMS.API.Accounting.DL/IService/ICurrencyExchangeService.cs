using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.Service.Models;
using ITL.NetCore.Connection.BL;
using System;
using System.Collections.Generic;

namespace eFMS.API.Accounting.DL.IService
{
    public interface ICurrencyExchangeService : IRepositoryBase<CatCurrencyExchange, CatCurrencyExchangeModel>
    {
        decimal GetRateCurrencyExchange(List<CatCurrencyExchange> currencyExchange, string currencyFrom, string currencyTo);

        decimal CurrencyExchangeRateConvert(decimal? finalExchangeRate, DateTime? exchangeDate, string currencyFrom, string currencyTo);

        decimal CalculatorAmount(decimal? amount, decimal? finalExchangeRate, DateTime? exchangeDate, string currencyFrom, string currencyTo, int? roundCurr);

        AmountResult CalculatorAmountAccountingByCurrency(string currencyCharge, decimal? vatRate, decimal? unitPrice, decimal quantity, decimal? finalExcRate, DateTime? excDate, string currencyConvert);
    }
}
