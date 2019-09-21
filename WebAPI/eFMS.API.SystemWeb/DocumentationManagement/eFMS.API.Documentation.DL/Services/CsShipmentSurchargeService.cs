using AutoMapper;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.Service.Contexts;
using eFMS.API.Documentation.Service.Models;
using eFMS.API.Documentation.Service.ViewModels;
using ITL.NetCore.Common;
using ITL.NetCore.Connection;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace eFMS.API.Documentation.DL.Services
{
    public class CsShipmentSurchargeService : RepositoryBase<CsShipmentSurcharge, CsShipmentSurchargeModel>, ICsShipmentSurchargeService
    {
        private readonly IStringLocalizer stringLocalizer;
        public CsShipmentSurchargeService(IContextBase<CsShipmentSurcharge> repository, IMapper mapper, IStringLocalizer<LanguageSub> localizer) : base(repository, mapper)
        {
            stringLocalizer = localizer;
        }

        public HandleState DeleteCharge(Guid chargeId)
        {
            var hs = new HandleState();
            try
            {
                var charge = DataContext.Where(x => x.Id == chargeId).FirstOrDefault();
                if (charge == null)
                    hs = new HandleState(stringLocalizer[LanguageSub.MSG_SURCHARGE_NOT_FOUND].Value);
                if (charge != null && (charge.CreditNo != null || charge.Soano != null))
                {
                    hs = new HandleState(stringLocalizer[LanguageSub.MSG_SURCHARGE_NOT_ALLOW_DELETED].Value);
                }
                else
                {
                    DataContext.Delete(x => x.Id == chargeId);
                }
            }
            catch(Exception ex)
            {
                hs = new HandleState(ex.Message);
            }
            return hs;
        }

        public List<CatPartner> GetAllParner(Guid id, bool IsHouseBillID = false)
        {
            try
            {
                List<CatPartner> listPartners = new List<CatPartner>();
                if (IsHouseBillID == false)
                {
                    List<Guid> lst_Hbid = ((eFMSDataContext)DataContext.DC).CsTransactionDetail.Where(x => x.JobId == id).ToList().Select(x => x.Id).ToList();
                    foreach (var idHB in lst_Hbid)
                    {
                        var houseBill = ((eFMSDataContext)DataContext.DC).CsTransactionDetail.Where(x => x.Id == idHB).FirstOrDefault();
                        List<CsShipmentSurchargeDetailsModel> listCharges = new List<CsShipmentSurchargeDetailsModel>();
                        if (houseBill != null)
                        {
                            listCharges = Query(houseBill.Id, null);

                            foreach (var c in listCharges)
                            {
                                if (c.PaymentObjectId != null)
                                {
                                    var partner = ((eFMSDataContext)DataContext.DC).CatPartner.Where(x => x.Id == c.PaymentObjectId).FirstOrDefault();
                                    if (partner != null) listPartners.Add(partner);
                                }
                                if (c.PayerId != null)
                                {
                                    var partner = ((eFMSDataContext)DataContext.DC).CatPartner.Where(x => x.Id == c.PayerId).FirstOrDefault();
                                    if (partner != null) listPartners.Add(partner);
                                }
                            }
                        }

                    }
                }
                else
                {
                    var houseBill = ((eFMSDataContext)DataContext.DC).CsTransactionDetail.Where(x => x.Id == id).FirstOrDefault();
                    List<CsShipmentSurchargeDetailsModel> listCharges = new List<CsShipmentSurchargeDetailsModel>();
                   
                    listCharges = Query(id, null);

                    foreach (var c in listCharges)
                    {
                        if (c.PaymentObjectId != null)
                        {
                            var partner = ((eFMSDataContext)DataContext.DC).CatPartner.Where(x => x.Id == c.PaymentObjectId).FirstOrDefault();
                            if (partner != null) listPartners.Add(partner);
                        }
                        if (c.PayerId != null)
                        {
                            var partner = ((eFMSDataContext)DataContext.DC).CatPartner.Where(x => x.Id == c.PayerId).FirstOrDefault();
                            if (partner != null) listPartners.Add(partner);
                        }
                    }
                }
                
                listPartners = listPartners.Distinct().ToList();
                return listPartners;
            }
            catch(Exception ex)
            {
                throw ex;
            }

        }


       

        public List<CsShipmentSurchargeDetailsModel> GetByHB(Guid HbID,string type)
        {
            return Query(HbID,type);
        }

        public List<object> GetByListHB(List<Guid> lst_Hbid)
        {
           List<object> returnList = new List<object>();
           foreach(var id in lst_Hbid)
            {
                var houseBill = ((eFMSDataContext)DataContext.DC).CsTransactionDetail.Where(x => x.Id == id).FirstOrDefault();
                List<CsShipmentSurchargeDetailsModel> listCharges = new List<CsShipmentSurchargeDetailsModel>();
                if (houseBill != null)
                {
                    listCharges = Query(houseBill.Id, null);
                }

                var returnObj = new { houseBill.Hwbno, houseBill.Hbltype, houseBill.Id, listCharges };
                returnList.Add(returnObj);
            }
            return returnList;
        }

        public List<object> GroupChargeByHB(Guid Id, string PartnerId, bool IsHouseBillID)
        {
            List<object> returnList = new List<object>();
            List<CsShipmentSurchargeDetailsModel> listCharges = new List<CsShipmentSurchargeDetailsModel>();
            if (IsHouseBillID == false)
            {
                List<Guid> lst_Hbid = ((eFMSDataContext)DataContext.DC).CsTransactionDetail.Where(x => x.JobId == Id).ToList().Select(x => x.Id).ToList();
                foreach (var id in lst_Hbid)
                {
                    var houseBill = ((eFMSDataContext)DataContext.DC).CsTransactionDetail.Where(x => x.Id == id).FirstOrDefault();
                    if (houseBill != null)
                    {
                        listCharges = Query(houseBill.Id, null);
                        listCharges = listCharges.Where(x => (x.PayerId == PartnerId || x.Type == "OBH" || x.PaymentObjectId == PartnerId)).ToList();
                    }
                    listCharges = listCharges.Where(x => (x.CreditNo == null || x.CreditNo.Trim() == "" || x.DebitNo == null || x.DebitNo.Trim() == "")).ToList();

                    foreach (var item in listCharges)
                    {
                        var exchangeRate = ((eFMSDataContext)DataContext.DC).CatCurrencyExchange.Where(x => (x.DatetimeCreated.Value.Date == item.ExchangeDate.Value.Date && x.CurrencyFromId == item.CurrencyId && x.CurrencyToId == "VND" && x.Inactive == false)).OrderByDescending(x => x.DatetimeModified).FirstOrDefault();
                        item.ExchangeRate = exchangeRate?.Rate;
                    }
                    var returnObj = new { houseBill.Hwbno, houseBill.Hbltype, houseBill.Id, listCharges };

                    returnList.Add(returnObj);

                }
            }
            else
            {
                var houseBill = ((eFMSDataContext)DataContext.DC).OpsTransaction.Where(x => x.Hblid == Id).FirstOrDefault();
                listCharges = Query(Id, null);
                listCharges = listCharges.Where(x => ((x.PayerId == PartnerId && x.Type == "OBH") || x.PaymentObjectId == PartnerId)).ToList();
                var returnObj = new { houseBill.Hwbno, houseBill.Id, listCharges };

                returnList.Add(returnObj);
            }

            return returnList;

        }

        //private List<CsShipmentSurchargeDetailsModel> Query(Guid HbID, string type)
        //{
        //    List<CsShipmentSurchargeDetailsModel> listCharges = new List<CsShipmentSurchargeDetailsModel>();
        //    var charges = DataContext.Get(x => x.Hblid == HbID);
        //    if (!string.IsNullOrEmpty(type))
        //    {
        //        charges = charges.Where(x => x.Type.ToLower() == type.ToLower());
        //    }
        //    var query = (from charge in charges
        //                 join chargeDetail in ((eFMSDataContext)DataContext.DC).CatCharge on charge.ChargeId equals chargeDetail.Id

        //                 join partner in ((eFMSDataContext)DataContext.DC).CatPartner on charge.PaymentObjectId equals partner.Id into partnerGroup
        //                 from p in partnerGroup.DefaultIfEmpty()

        //                 join payer in ((eFMSDataContext)DataContext.DC).CatPartner on charge.PayerId equals payer.Id into payerGroup
        //                 from pay in payerGroup.DefaultIfEmpty()

        //                 join unit in ((eFMSDataContext)DataContext.DC).CatUnit on charge.UnitId equals unit.Id
        //                 join currency in ((eFMSDataContext)DataContext.DC).CatCurrency on charge.CurrencyId equals currency.Id
        //                 select new { charge, p, pay, unit.UnitNameEn, CurrencyCode = currency.Id, chargeDetail.ChargeNameEn, chargeDetail.Code }
        //                ).ToList();
        //    foreach (var item in query)
        //    {
               
                
        //        var charge = mapper.Map<CsShipmentSurchargeDetailsModel>(item.charge);
        //        if(charge.ExchangeDate != null)
        //        {
        //            var exchangeCurrencyToVND = ((eFMSDataContext)DataContext.DC).CatCurrencyExchange.Where(x => (x.DatetimeCreated.Value.Date == charge.ExchangeDate.Value.Date 
        //                && x.CurrencyFromId == charge.CurrencyId 
        //                && x.CurrencyToId == "VND" && x.Inactive == false)).OrderByDescending(x => x.DatetimeModified).FirstOrDefault();
        //            var exchangeUSDToVND = ((eFMSDataContext)DataContext.DC).CatCurrencyExchange.Where(x => (x.DatetimeCreated.Value.Date == charge.ExchangeDate.Value.Date 
        //                && x.CurrencyFromId == "USD" 
        //                && x.CurrencyToId == "VND" && x.Inactive == false)).OrderByDescending(x => x.DatetimeModified).FirstOrDefault();
        //            charge.ExchangeRate = exchangeCurrencyToVND == null ? 1 : exchangeCurrencyToVND.Rate;
        //            charge.ExchangeRateUSDToVND = exchangeUSDToVND == null ? 1 : exchangeUSDToVND.Rate;
        //        }
        //        else
        //        {
        //            charge.ExchangeRate = 1;
        //            charge.ExchangeRateUSDToVND = 1;
        //        }
        //        charge.PartnerName = item.p?.PartnerNameEn;
        //        charge.NameEn = item?.ChargeNameEn;
        //        if(charge.Type == "OBH")
        //        {
        //            charge.ReceiverName = item.p?.ShortName;
        //        }
        //        charge.PayerName = item.pay?.ShortName;
        //        charge.Unit = item.UnitNameEn;
        //        charge.Currency = item.CurrencyCode;
        //        charge.ChargeCode = item.Code;
        //        listCharges.Add(charge);
        //    }
        //    return listCharges;
        //}

        public ChargeShipmentResult GetListChargeShipment(ChargeShipmentCriteria criteria)
        {
            var chargeShipmentList = GetSpcChargeShipment(criteria).ToList();
            var dataMap = mapper.Map<List<spc_GetListChargeShipmentMaster>,List<ChargeShipmentModel>>(chargeShipmentList);
            var result = new ChargeShipmentResult
            {
                ChargeShipments = dataMap,
                TotalShipment = chargeShipmentList.Where(x=>x.HBL != null).GroupBy(x=>x.HBL).Count(),
                TotalCharge = chargeShipmentList.Count(),
                AmountDebitLocal = chargeShipmentList.Sum(x=>x.AmountDebitLocal),
                AmountCreditLocal = chargeShipmentList.Sum(x=>x.AmountCreditLocal),
                AmountDebitUSD = chargeShipmentList.Sum(x=>x.AmountDebitUSD),
                AmountCreditUSD = chargeShipmentList.Sum(x=>x.AmountCreditUSD),
            };
            return result;
        }
        List<CsShipmentSurchargeDetailsModel> Query(Guid HbID, string type)
        {
            if (type == null) type = string.Empty;
            List<CsShipmentSurchargeDetailsModel> listCharges = new List<CsShipmentSurchargeDetailsModel>();
            var query = GetChargeByHouseBill(HbID, type);
            if (query.Count == 0) return listCharges;
            var exchangRates = ((eFMSDataContext)DataContext.DC).CatCurrencyExchange.Where(x => x.Inactive == false).ToList();
            foreach (var item in query)
            {
                var charge = mapper.Map<CsShipmentSurchargeDetailsModel>(item);
                charge.Currency = item.CurrencyCode;
                charge.Unit = item.UnitNameEn;
                charge.NameEn = item.ChargeNameEn;
                if (charge.ExchangeDate != null)
                {
                    var exchangeCurrencyToVND = exchangRates.Where(x => (x.DatetimeCreated.Value.Date == charge.ExchangeDate.Value.Date
                        && x.CurrencyFromId == charge.CurrencyId
                        && x.CurrencyToId == "VND")).OrderByDescending(x => x.DatetimeModified).FirstOrDefault();
                    var exchangeUSDToVND = exchangRates.Where(x => (x.DatetimeCreated.Value.Date == charge.ExchangeDate.Value.Date
                        && x.CurrencyFromId == "USD"
                        && x.CurrencyToId == "VND")).OrderByDescending(x => x.DatetimeModified).FirstOrDefault();
                    charge.ExchangeRate = exchangeCurrencyToVND == null ? 1 : exchangeCurrencyToVND.Rate;
                    charge.ExchangeRateUSDToVND = exchangeUSDToVND == null ? 1 : exchangeUSDToVND.Rate;
                }
                else
                {
                    charge.ExchangeRate = 1;
                    charge.ExchangeRateUSDToVND = 1;
                }
                listCharges.Add(charge);
            }
            return listCharges;
        }
        private List<spc_GetSurchargeByHouseBill> GetChargeByHouseBill(Guid id, string type)
        {
            var parameters = new[]{
                new SqlParameter(){ ParameterName="@hblid", Value = id.ToString() },
                new SqlParameter(){ ParameterName="@type", Value = type } 
            };
            var list = ((eFMSDataContext)DataContext.DC).ExecuteProcedure<spc_GetSurchargeByHouseBill>(parameters);
            return list;
        }
        private List<spc_GetListChargeShipmentMaster> GetSpcChargeShipment(ChargeShipmentCriteria criteria)
        {
            DbParameter[] parameters =
            {
                SqlParam.GetParameter("currencyLocal", criteria.CurrencyLocal),
                SqlParam.GetParameter("customerID", criteria.CustomerID),
                SqlParam.GetParameter("dateType", criteria.DateType),
                SqlParam.GetParameter("fromDate", criteria.FromDate),
                SqlParam.GetParameter("toDate", criteria.ToDate),
                SqlParam.GetParameter("type", criteria.Type),
                SqlParam.GetParameter("isOBH", criteria.IsOBH),
                SqlParam.GetParameter("strCreators", criteria.StrCreators),
                SqlParam.GetParameter("strCharges", criteria.StrCharges),
                SqlParam.GetParameter("commodityGroupID", criteria.CommodityGroupID),
                SqlParam.GetParameter("strServices", criteria.StrServices)
            };
            return ((eFMSDataContext)DataContext.DC).ExecuteProcedure<spc_GetListChargeShipmentMaster>(parameters);
        }

        public List<CsShipmentSurchargeDetailsModel> GetByHB(Guid hblid)
        {
            List<CsShipmentSurchargeDetailsModel> results = new List<CsShipmentSurchargeDetailsModel>();
            var data = GetChargeByHouseBill(hblid, string.Empty);
            if (data.Count == 0) return results;
            results = mapper.Map<List<CsShipmentSurchargeDetailsModel>>(data);
            return results;
        }
    }
}
