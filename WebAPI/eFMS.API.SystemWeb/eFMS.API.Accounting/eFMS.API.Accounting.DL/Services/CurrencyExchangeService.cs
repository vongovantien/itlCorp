using AutoMapper;
using eFMS.API.Accounting.DL.Common;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.Service.Models;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Accounting.DL.Services
{
    public class CurrencyExchangeService : RepositoryBase<CatCurrencyExchange, CatCurrencyExchangeModel>, ICurrencyExchangeService
    {        
        public CurrencyExchangeService(IContextBase<CatCurrencyExchange> repository, IMapper mapper) : base(repository, mapper)
        {
        }

        public decimal GetRateCurrencyExchange(List<CatCurrencyExchange> currencyExchange, string currencyFrom, string currencyTo)
        {
            if (currencyExchange.Count == 0 || string.IsNullOrEmpty(currencyFrom) || string.IsNullOrEmpty(currencyTo)) return 0;

            currencyFrom = currencyFrom.Trim();
            currencyTo = currencyTo.Trim();

            if (currencyFrom != currencyTo)
            {
                var get1 = currencyExchange.Where(x => x.CurrencyFromId.Trim() == currencyFrom && x.CurrencyToId.Trim() == currencyTo).OrderByDescending(x => x.Rate).FirstOrDefault();
                if (get1 != null)
                {
                    return get1.Rate;
                }
                else
                {
                    var get2 = currencyExchange.Where(x => x.CurrencyFromId.Trim() == currencyTo && x.CurrencyToId.Trim() == currencyFrom).OrderByDescending(x => x.Rate).FirstOrDefault();
                    if (get2 != null)
                    {
                        return 1 / get2.Rate;
                    }
                    else
                    {
                        var get3 = currencyExchange.Where(x => x.CurrencyFromId.Trim() == currencyFrom || x.CurrencyFromId.Trim() == currencyTo).OrderByDescending(x => x.Rate).ToList();
                        if (get3.Count > 1)
                        {
                            if (get3[0].CurrencyFromId.Trim() == currencyFrom && get3[1].CurrencyFromId.Trim() == currencyTo)
                            {
                                return get3[0].Rate / get3[1].Rate;
                            }
                            else
                            {
                                return get3[1].Rate / get3[0].Rate;
                            }
                        }
                        else
                        {
                            //Nến không tồn tại Currency trong Exchange thì return về 0
                            return 0;
                        }
                    }
                }
            }
            return 1;
        }

        public decimal CurrencyExchangeRateConvert(decimal? finalExchangeRate, DateTime? exchangeDate, string currencyFrom, string currencyTo)
        {
            if (string.IsNullOrEmpty(currencyFrom) || string.IsNullOrEmpty(currencyTo)) return 0;

            var exchargeDateSurcharge = exchangeDate == null ? DateTime.Now.Date : exchangeDate.Value.Date;
            List<CatCurrencyExchange> currencyExchange = DataContext.Get(x => x.DatetimeCreated.Value.Date == exchargeDateSurcharge).ToList();
            decimal _exchangeRateCurrencyTo = GetRateCurrencyExchange(currencyExchange, currencyTo, AccountingConstants.CURRENCY_LOCAL); //Lấy currency Local làm gốc để quy đỗi
            decimal _exchangeRateCurrencyFrom = GetRateCurrencyExchange(currencyExchange, currencyFrom, AccountingConstants.CURRENCY_LOCAL); //Lấy currency Local làm gốc để quy đỗi

            decimal _exchangeRate = 0;
            if (finalExchangeRate != null)
            {
                if (currencyFrom == currencyTo)
                {
                    _exchangeRate = 1;
                }
                else if (currencyFrom == AccountingConstants.CURRENCY_LOCAL && currencyTo != AccountingConstants.CURRENCY_LOCAL)
                {
                    _exchangeRate = (finalExchangeRate.Value != 0) ? (1 / finalExchangeRate.Value) : 0;
                }
                else if (currencyFrom != AccountingConstants.CURRENCY_LOCAL && currencyTo == AccountingConstants.CURRENCY_LOCAL)
                {
                    _exchangeRate = finalExchangeRate.Value;
                }
                else
                {
                    _exchangeRate = (_exchangeRateCurrencyTo != 0) ? (finalExchangeRate.Value / _exchangeRateCurrencyTo) : 0;
                }
            }
            else
            {
                if (currencyFrom == currencyTo)
                {
                    _exchangeRate = 1;
                }
                else if (currencyFrom == AccountingConstants.CURRENCY_LOCAL && currencyTo != AccountingConstants.CURRENCY_LOCAL)
                {
                    _exchangeRate = (_exchangeRateCurrencyTo != 0) ? (1 / _exchangeRateCurrencyTo) : 0;
                }
                else if (currencyFrom != AccountingConstants.CURRENCY_LOCAL && currencyTo == AccountingConstants.CURRENCY_LOCAL)
                {
                    _exchangeRate = GetRateCurrencyExchange(currencyExchange, currencyFrom, AccountingConstants.CURRENCY_LOCAL);
                }
                else
                {
                    _exchangeRate = (_exchangeRateCurrencyTo != 0) ? (_exchangeRateCurrencyFrom / _exchangeRateCurrencyTo) : 0;
                }
            }
            return _exchangeRate;
        }
    }
}
