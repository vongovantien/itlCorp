﻿using AutoMapper;
using eFMS.API.Common.Helpers;
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
        readonly IContextBase<CsShipmentSurcharge> surchargeRepository;
        public CurrencyExchangeService(IContextBase<CatCurrencyExchange> repository, IMapper mapper, IContextBase<CsShipmentSurcharge> surchargeRepo) : base(repository, mapper)
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
                //Order giảm dần theo ngày tạo
                var get1 = currencyExchange.Where(x => x.CurrencyFromId.Trim() == currencyFrom && x.CurrencyToId.Trim() == currencyTo).OrderByDescending(x => x.DatetimeCreated).FirstOrDefault();
                if (get1 != null)
                {
                    return get1.Rate;
                }
                else
                {
                    //Order giảm dần theo ngày tạo
                    var get2 = currencyExchange.Where(x => x.CurrencyFromId.Trim() == currencyTo && x.CurrencyToId.Trim() == currencyFrom).OrderByDescending(x => x.DatetimeCreated).FirstOrDefault();
                    if (get2 != null)
                    {
                        return 1 / get2.Rate;
                    }
                    else
                    {
                        //Order giảm dần theo ngày tạo
                        var get3 = currencyExchange.Where(x => x.CurrencyFromId.Trim() == currencyFrom || x.CurrencyFromId.Trim() == currencyTo).OrderByDescending(x => x.DatetimeCreated).ToList();
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
                if (currencyFrom != ForPartnerConstants.CURRENCY_LOCAL && currencyTo == ForPartnerConstants.CURRENCY_LOCAL)
                {
                    return finalExchangeRate.Value;
                }
            }
            //***

            var currencyExcLookup = DataContext.Get().ToLookup(x => x.DatetimeCreated.Value.Date);

            DateTime? maxDateCreated = DataContext.Get().Max(s => s.DatetimeCreated);
            var exchargeDateSurcharge = exchangeDate == null ? maxDateCreated : exchangeDate.Value.Date;
            //List<CatCurrencyExchange> currencyExchange = DataContext.Get(x => x.DatetimeCreated.Value.Date == exchargeDateSurcharge).ToList();
            var currencyExchange = currencyExcLookup[exchargeDateSurcharge.Value.Date].ToList();

            if (currencyExchange.Count == 0)
            {
                //currencyExchange = DataContext.Get(x => x.DatetimeCreated.Value.Date == maxDateCreated.Value.Date).ToList();
                currencyExchange = currencyExcLookup[maxDateCreated.Value.Date].ToList();
            }

            decimal _exchangeRateCurrencyTo = GetRateCurrencyExchange(currencyExchange, currencyTo, ForPartnerConstants.CURRENCY_LOCAL); //Lấy currency Local làm gốc để quy đỗi
            decimal _exchangeRateCurrencyFrom = GetRateCurrencyExchange(currencyExchange, currencyFrom, ForPartnerConstants.CURRENCY_LOCAL); //Lấy currency Local làm gốc để quy đỗi

            decimal _exchangeRate = 0;
            if (finalExchangeRate != null)
            {
                //if (currencyFrom == currencyTo)
                //{
                //    _exchangeRate = 1;
                //}
                //else 
                if (currencyFrom == ForPartnerConstants.CURRENCY_LOCAL && currencyTo != ForPartnerConstants.CURRENCY_LOCAL)
                {
                    _exchangeRate = (_exchangeRateCurrencyTo != 0) ? (1 / _exchangeRateCurrencyTo) : 0;
                }
                //else if (currencyFrom != ForPartnerConstants.CURRENCY_LOCAL && currencyTo == ForPartnerConstants.CURRENCY_LOCAL)
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
                if (currencyFrom == ForPartnerConstants.CURRENCY_LOCAL && currencyTo != ForPartnerConstants.CURRENCY_LOCAL)
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
        /// <param name="surcharge"></param>
        /// <param name="currencyConvert"></param>
        /// <returns></returns>
        public AmountResult CalculatorAmountAccountingByCurrency(CsShipmentSurcharge surcharge, string currencyConvert)
        {
            AmountResult amountResult = new AmountResult();
            int _roundDecimal = currencyConvert == ForPartnerConstants.CURRENCY_LOCAL ? 0 : 2; //Local round 0, ngoại tệ round 2
            decimal _netAmount = 0;
            decimal _vatAmount = 0;
            decimal _excRate = 0;

            //Tính tỉ giá Final Exchange Rate (Tỉ giá so với LOCAL)
            var exchangeRateToLocal = CurrencyExchangeRateConvert(surcharge.FinalExchangeRate, surcharge.ExchangeDate, surcharge.CurrencyId, ForPartnerConstants.CURRENCY_LOCAL);
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
            if (currencyObject == ForPartnerConstants.CURRENCY_LOCAL)
            {
                _totalAmount = (surcharge.AmountVnd + surcharge.VatAmountVnd) ?? 0;
            }
            else if (currencyObject == ForPartnerConstants.CURRENCY_USD)
            {
                _totalAmount = (surcharge.AmountUsd + surcharge.VatAmountUsd) ?? 0;
            }
            else if (currencyObject == surcharge.CurrencyId)
            {
                _totalAmount = surcharge.Total;
            }
            else //Ngoại tệ khác
            {
                decimal _exchangeRate = CurrencyExchangeRateConvert(surcharge.FinalExchangeRate, surcharge.ExchangeDate, surcharge.CurrencyId, currencyObject);
                decimal _netAmount = NumberHelper.RoundNumber((surcharge.UnitPrice * surcharge.Quantity * _exchangeRate) ?? 0, 2);
                decimal _vatAmount = 0;
                if (surcharge.Vatrate != null)
                {
                    decimal vatAmount = surcharge.Vatrate < 0 ? Math.Abs(surcharge.Vatrate ?? 0) : ((surcharge.UnitPrice * surcharge.Quantity * surcharge.Vatrate) ?? 0) / 100;
                    _vatAmount = NumberHelper.RoundNumber(vatAmount * _exchangeRate, 2);
                }
                _totalAmount = _netAmount + _vatAmount;
            }
            return _totalAmount;
        }

        /// <summary>
        /// Tính toán giá trị các field: NetAmount, Total, FinalExchangeRate, AmountVnd, VatAmountVnd, AmountUsd, VatAmountUsd
        /// </summary>
        /// <param name="surcharge"></param>
        /// <returns></returns>
        public AmountSurchargeResult CalculatorAmountSurcharge(CsShipmentSurcharge surcharge)
        {
            AmountSurchargeResult result = new AmountSurchargeResult();
            var amountOriginal = CalculatorAmountAccountingByCurrency(surcharge, surcharge.CurrencyId);
            result.NetAmountOrig = amountOriginal.NetAmount; //Thành tiền trước thuế (Original)
            result.VatAmountOrig = amountOriginal.VatAmount; //Tiền thuế (Original)
            result.GrossAmountOrig = amountOriginal.NetAmount + amountOriginal.VatAmount; //Thành tiền sau thuế (Original)
            result.FinalExchangeRate = amountOriginal.ExchangeRate; //Tỉ giá so với Local

            if (surcharge.CurrencyId == ForPartnerConstants.CURRENCY_LOCAL)
            {
                result.AmountVnd = amountOriginal.NetAmount;
                result.VatAmountVnd = amountOriginal.VatAmount;
            }
            else
            {
                var amountLocal = CalculatorAmountAccountingByCurrency(surcharge, ForPartnerConstants.CURRENCY_LOCAL);
                result.AmountVnd = amountLocal.NetAmount; //Thành tiền trước thuế (Local)
                result.VatAmountVnd = amountLocal.VatAmount; //Tiền thuế (Local)
            }

            if (surcharge.CurrencyId == ForPartnerConstants.CURRENCY_USD)
            {
                result.AmountUsd = amountOriginal.NetAmount;
                result.VatAmountUsd = amountOriginal.VatAmount;
            }
            else
            {
                var amountUsd = CalculatorAmountAccountingByCurrency(surcharge, ForPartnerConstants.CURRENCY_USD);
                result.AmountUsd = amountUsd.NetAmount; //Thành tiền trước thuế (USD)
                result.VatAmountUsd = amountUsd.VatAmount; //Tiền thuế (USD)
            }
            return result;
        }
    }
}
