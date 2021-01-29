using AutoMapper;
using eFMS.API.Accounting.DL.Common;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.Service.Models;
using eFMS.API.Common.Helpers;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Accounting.DL.Services
{
    public class CurrencyExchangeService : RepositoryBase<CatCurrencyExchange, CatCurrencyExchangeModel>, ICurrencyExchangeService
    {
        readonly IContextBase<CsShipmentSurcharge> surchargeRepository;
        public CurrencyExchangeService(IContextBase<CatCurrencyExchange> repository, 
            IMapper mapper, 
            IContextBase<CsShipmentSurcharge> surchargeRepo) : base(repository, mapper)
        {
            surchargeRepository = surchargeRepo;
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

            //***
            if (currencyFrom == currencyTo)
            {
                return 1;
            }
            if (finalExchangeRate != null)
            {
                if (currencyFrom != AccountingConstants.CURRENCY_LOCAL && currencyTo == AccountingConstants.CURRENCY_LOCAL)
                {
                    return finalExchangeRate.Value;
                }
            }
            //***

            var exchargeDateSurcharge = exchangeDate == null ? DataContext.Get().Max(s => s.DatetimeCreated).Value.Date : exchangeDate.Value.Date;
            List<CatCurrencyExchange> currencyExchange = DataContext.Get(x => x.DatetimeCreated.Value.Date == exchargeDateSurcharge).ToList();
            if (currencyExchange.Count() == 0)
            {
                DateTime? maxDateCreated = DataContext.Get().Max(s => s.DatetimeCreated);
                currencyExchange = DataContext.Get(x => x.DatetimeCreated.Value.Date == maxDateCreated.Value.Date).ToList();
            }

            decimal _exchangeRateCurrencyTo = GetRateCurrencyExchange(currencyExchange, currencyTo, AccountingConstants.CURRENCY_LOCAL); //Lấy currency Local làm gốc để quy đỗi
            decimal _exchangeRateCurrencyFrom = GetRateCurrencyExchange(currencyExchange, currencyFrom, AccountingConstants.CURRENCY_LOCAL); //Lấy currency Local làm gốc để quy đỗi

            decimal _exchangeRate = 0;
            if (finalExchangeRate != null)
            {
                //if (currencyFrom == currencyTo)
                //{
                //    _exchangeRate = 1;
                //}
                //else 
                if (currencyFrom == AccountingConstants.CURRENCY_LOCAL && currencyTo != AccountingConstants.CURRENCY_LOCAL)
                {
                    _exchangeRate = (_exchangeRateCurrencyTo != 0) ? (1 / _exchangeRateCurrencyTo) : 0;
                }
                //else if (currencyFrom != AccountingConstants.CURRENCY_LOCAL && currencyTo == AccountingConstants.CURRENCY_LOCAL)
                //{
                //    _exchangeRate = finalExchangeRate.Value;
                //}
                else
                {
                    _exchangeRate = (_exchangeRateCurrencyTo != 0) ? (finalExchangeRate.Value / _exchangeRateCurrencyTo) : 0;
                }
            }
            else
            {
                //if (currencyFrom == currencyTo)
                //{
                //    _exchangeRate = 1;
                //}
                //else 
                if (currencyFrom == AccountingConstants.CURRENCY_LOCAL && currencyTo != AccountingConstants.CURRENCY_LOCAL)
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
            if (currencyTo == AccountingConstants.CURRENCY_LOCAL)
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

            decimal _exchangeRateCurrencyFrom = GetRateCurrencyExchange(currencyExchange, currencyFrom, AccountingConstants.CURRENCY_LOCAL); //Lấy currency Local làm gốc để quy đỗi
            decimal _exchangeRateCurrencyTo = GetRateCurrencyExchange(currencyExchange, currencyTo, AccountingConstants.CURRENCY_LOCAL); //Lấy currency Local làm gốc để quy đỗi

            //Tránh case chia 0
            _exchangeRateCurrencyTo = _exchangeRateCurrencyTo == 0 ? 1 : _exchangeRateCurrencyTo;

            decimal roundCurrAmount = NumberHelper.RoundNumber(amount.Value, roundCurr.Value);
            if (finalExchangeRate != null)
            {
                //RoundCurr (RoundLocal(RoundCurr(Amount) x  FinalExc)/ExcByDate (Currency 2) ) 
                decimal roundCurrAmountFinal = NumberHelper.RoundNumber(roundCurrAmount * finalExchangeRate.Value, roundLocal);
                amountResult = NumberHelper.RoundNumber(roundCurrAmountFinal / _exchangeRateCurrencyTo, roundCurr.Value);
            }
            else
            {
                //RoundCurr (RoundLocal(RoundCurr(Amount) x  ExcByDate (Currency 1) )/ExcByDate (Currency 2) )  
                decimal roundCurrAmountExcDate = NumberHelper.RoundNumber(roundCurrAmount * _exchangeRateCurrencyFrom, roundLocal);
                amountResult = NumberHelper.RoundNumber(roundCurrAmountExcDate / _exchangeRateCurrencyTo, roundCurr.Value);
            }

            return amountResult;
        }

        /// <summary>
        /// Get NetAmount, VatAmount, ExchangeRate (to Local)
        /// </summary>
        /// <param name="currencyCharge"></param>
        /// <param name="vatRate"></param>
        /// <param name="unitPrice"></param>
        /// <param name="quantity"></param>
        /// <param name="finalExcRate"></param>
        /// <param name="excDate"></param>
        /// <param name="currencyConvert"></param>
        /// <returns></returns>
        public AmountResult CalculatorAmountAccountingByCurrency(CsShipmentSurcharge surcharge, string currencyConvert)
        {
            AmountResult amountResult = new AmountResult();
            int _roundDecimal = currencyConvert == AccountingConstants.CURRENCY_LOCAL ? 0 : 2; //Local round 0, ngoại tệ round 2
            decimal _netAmount = 0;
            decimal _vatAmount = 0;
            decimal _excRate = 0;

            //Tính tỉ giá Final Exchange Rate (Tỉ giá so với LOCAL)
            var exchangeRateToLocal = CurrencyExchangeRateConvert(surcharge.FinalExchangeRate, surcharge.ExchangeDate, surcharge.CurrencyId, AccountingConstants.CURRENCY_LOCAL);
            _excRate = exchangeRateToLocal;

            if (surcharge.CurrencyId == currencyConvert)
            {
                _netAmount = NumberHelper.RoundNumber((surcharge.UnitPrice * surcharge.Quantity) ?? 0, _roundDecimal);
                if (surcharge.Vatrate != null)
                {
                    var vatAmount = surcharge.Vatrate < 0 ? Math.Abs(surcharge.Vatrate ?? 0) : ((surcharge.UnitPrice * surcharge.Quantity * surcharge.Vatrate) ?? 0) / 100;
                    _vatAmount = NumberHelper.RoundNumber(vatAmount, _roundDecimal);
                }
            }
            else
            {
                var exchangeRate = CurrencyExchangeRateConvert(surcharge.FinalExchangeRate, surcharge.ExchangeDate, surcharge.CurrencyId, currencyConvert);
                _netAmount = NumberHelper.RoundNumber((surcharge.UnitPrice * surcharge.Quantity * exchangeRate) ?? 0, _roundDecimal);
                if (surcharge.Vatrate != null)
                {
                    var vatAmount = surcharge.Vatrate < 0 ? Math.Abs(surcharge.Vatrate ?? 0) : ((surcharge.UnitPrice * surcharge.Quantity * surcharge.Vatrate) ?? 0) / 100;
                    _vatAmount = NumberHelper.RoundNumber(vatAmount * exchangeRate, _roundDecimal);
                }
            }
            amountResult.NetAmount = _netAmount;
            amountResult.VatAmount = _vatAmount;
            amountResult.ExchangeRate = _excRate;
            return amountResult;
        }

        public decimal ConvertAmountChargeToAmountObj(CsShipmentSurcharge surcharge, string currencyObject)
        {
            decimal _totalAmount = 0;
            if (currencyObject == AccountingConstants.CURRENCY_LOCAL)
            {
                _totalAmount = (surcharge.AmountVnd + surcharge.VatAmountVnd) ?? 0;
            }
            else if (currencyObject == AccountingConstants.CURRENCY_USD)
            {
                _totalAmount = (surcharge.AmountUsd + surcharge.VatAmountUsd) ?? 0;
            }
            else if (currencyObject == surcharge.CurrencyId)
            {
                _totalAmount = surcharge.Total;
            }
            else //SOA ngoại tệ khác
            {
                var _exchangeRate = CurrencyExchangeRateConvert(surcharge.FinalExchangeRate, surcharge.ExchangeDate, surcharge.CurrencyId, currencyObject);
                _totalAmount = surcharge.Total * _exchangeRate;
            }
            return _totalAmount;
        }
    }
}
