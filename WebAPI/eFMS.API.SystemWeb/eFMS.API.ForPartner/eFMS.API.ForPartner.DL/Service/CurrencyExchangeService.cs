using AutoMapper;
using eFMS.API.ForPartner.DL.Common;
using eFMS.API.ForPartner.DL.IService;
using eFMS.API.ForPartner.DL.Models;
using eFMS.API.ForPartner.Service.Models;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.ForPartner.DL.Service
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

            DateTime? maxDateCreated = DataContext.Get().Max(s => s.DatetimeCreated);
            var exchargeDateSurcharge = exchangeDate == null ? maxDateCreated : exchangeDate.Value.Date;
            List<CatCurrencyExchange> currencyExchange = DataContext.Get(x => x.DatetimeCreated.Value.Date == exchargeDateSurcharge).ToList();
            if (currencyExchange.Count == 0)
            {
                currencyExchange = DataContext.Get(x => x.DatetimeCreated.Value.Date == maxDateCreated.Value.Date).ToList();
            }

            decimal _exchangeRateCurrencyTo = GetRateCurrencyExchange(currencyExchange, currencyTo, ForPartnerConstants.CURRENCY_LOCAL); //Lấy currency Local làm gốc để quy đỗi
            decimal _exchangeRateCurrencyFrom = GetRateCurrencyExchange(currencyExchange, currencyFrom, ForPartnerConstants.CURRENCY_LOCAL); //Lấy currency Local làm gốc để quy đỗi

            decimal _exchangeRate = 0;
            if (finalExchangeRate != null)
            {
                if (currencyFrom == currencyTo)
                {
                    _exchangeRate = 1;
                }
                else if (currencyFrom == ForPartnerConstants.CURRENCY_LOCAL && currencyTo != ForPartnerConstants.CURRENCY_LOCAL)
                {
                    _exchangeRate = (_exchangeRateCurrencyTo != 0) ? (1 / _exchangeRateCurrencyTo) : 0;
                }
                else if (currencyFrom != ForPartnerConstants.CURRENCY_LOCAL && currencyTo == ForPartnerConstants.CURRENCY_LOCAL)
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
                else if (currencyFrom == ForPartnerConstants.CURRENCY_LOCAL && currencyTo != ForPartnerConstants.CURRENCY_LOCAL)
                {
                    _exchangeRate = (_exchangeRateCurrencyTo != 0) ? (1 / _exchangeRateCurrencyTo) : 0;
                }
                else if (currencyFrom != ForPartnerConstants.CURRENCY_LOCAL && currencyTo == ForPartnerConstants.CURRENCY_LOCAL)
                {
                    _exchangeRate = GetRateCurrencyExchange(currencyExchange, currencyFrom, ForPartnerConstants.CURRENCY_LOCAL);
                }
                else
                {
                    _exchangeRate = (_exchangeRateCurrencyTo != 0) ? (_exchangeRateCurrencyFrom / _exchangeRateCurrencyTo) : 0;
                }
            }
            return _exchangeRate;
        }

        /// <summary>
        /// Tính giá trị Amount dựa vào Final Exchange  Rate, Exchange Date, Currency From, Currency To => có làm tròn giá trị
        /// </summary>
        /// <param name="amount">Amount</param>
        /// <param name="finalExchangeRate">Final Exchange Rate</param>
        /// <param name="exchangeDate">Exchange Date</param>
        /// <param name="currencyFrom">Currency From</param>
        /// <param name="currencyTo">Currency To</param>
        /// <param name="roundCurr">Default: làm tròn 2 chữ số thập phân (lấy 2 chữ số thập phân)</param>
        /// <returns></returns>
        public decimal CalculatorAmount(decimal? amount, decimal? finalExchangeRate, DateTime? exchangeDate, string currencyFrom, string currencyTo, int? roundCurr)
        {
            roundCurr = roundCurr == null ? 2 : roundCurr;
            decimal amountResult = 0;
            if (string.IsNullOrEmpty(currencyFrom) || string.IsNullOrEmpty(currencyTo)) return 0;
            int roundLocal = 0;
            if (currencyTo == ForPartnerConstants.CURRENCY_LOCAL)
            {
                roundCurr = 0;
            }
            DateTime? maxDateCreated = DataContext.Get().Max(s => s.DatetimeCreated);
            var exchargeDateSurcharge = exchangeDate == null ? maxDateCreated : exchangeDate.Value.Date;
            List<CatCurrencyExchange> currencyExchange = DataContext.Get(x => x.DatetimeCreated.Value.Date == exchargeDateSurcharge).ToList();
            if (currencyExchange.Count == 0)
            {
                currencyExchange = DataContext.Get(x => x.DatetimeCreated.Value.Date == maxDateCreated.Value.Date).ToList();
            }

            decimal _exchangeRateCurrencyFrom = GetRateCurrencyExchange(currencyExchange, currencyFrom, ForPartnerConstants.CURRENCY_LOCAL); //Lấy currency Local làm gốc để quy đỗi
            decimal _exchangeRateCurrencyTo = GetRateCurrencyExchange(currencyExchange, currencyTo, ForPartnerConstants.CURRENCY_LOCAL); //Lấy currency Local làm gốc để quy đỗi

            //Tránh case chia 0
            _exchangeRateCurrencyTo = _exchangeRateCurrencyTo == 0 ? 1 : _exchangeRateCurrencyTo;

            decimal roundCurrAmount = Math.Round(amount.Value, roundCurr.Value);
            if (finalExchangeRate != null)
            {
                //RoundCurr (RoundLocal(RoundCurr(Amount) x  FinalExc)/ExcByDate (Currency 2) ) 
                decimal roundCurrAmountFinal = Math.Round(roundCurrAmount * finalExchangeRate.Value, roundLocal);
                amountResult = Math.Round(roundCurrAmountFinal / _exchangeRateCurrencyTo, roundCurr.Value);
            }
            else
            {
                //RoundCurr (RoundLocal(RoundCurr(Amount) x  ExcByDate (Currency 1) )/ExcByDate (Currency 2) )  
                decimal roundCurrAmountExcDate = Math.Round(roundCurrAmount * _exchangeRateCurrencyFrom, roundLocal);
                amountResult = Math.Round(roundCurrAmountExcDate / _exchangeRateCurrencyTo, roundCurr.Value);
            }

            return amountResult;
        }
    }
}
