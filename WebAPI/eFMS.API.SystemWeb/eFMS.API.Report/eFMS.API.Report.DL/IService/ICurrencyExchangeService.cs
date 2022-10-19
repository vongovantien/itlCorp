﻿using eFMS.API.Report.DL.Models;
using eFMS.API.Report.Service.Models;
using ITL.NetCore.Connection.BL;
using System;
using System.Linq;

namespace eFMS.API.Report.DL.IService
{
    public interface ICurrencyExchangeService : IRepositoryBase<CatCurrencyExchange, CatCurrencyExchangeModel>
    {
        decimal GetRateCurrencyExchange(IQueryable<CatCurrencyExchange> currencyExchange, string currencyFrom, string currencyTo);

        decimal CurrencyExchangeRateConvert(decimal? finalExchangeRate, DateTime? exchangeDate, string currencyFrom, string currencyTo);

        decimal CalculatorAmount(decimal? amount, decimal? finalExchangeRate, DateTime? exchangeDate, string currencyFrom, string currencyTo, int? roundCurr);

        AmountResult CalculatorAmountAccountingByCurrency(CsShipmentSurcharge surcharge, string currencyConvert, decimal kickBackExcRate);

        decimal ConvertAmountChargeToAmountObj(CsShipmentSurcharge surcharge, string currencyObject);

        decimal ConvertNetAmountChargeToNetAmountObj(CsShipmentSurcharge surcharge, string currencyObject);

        AmountSurchargeResult CalculatorAmountSurcharge(CsShipmentSurcharge surcharge, decimal KickBackExcRate);
    }
}
