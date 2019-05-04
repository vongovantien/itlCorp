using AutoMapper;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.Service.Contexts;
using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Documentation.DL.Services
{
    public class CsShipmentSurchargeService : RepositoryBase<CsShipmentSurcharge, CsShipmentSurchargeModel>, ICsShipmentSurchargeService
    {
        public CsShipmentSurchargeService(IContextBase<CsShipmentSurcharge> repository, IMapper mapper) : base(repository, mapper)
        {

        }
        public override HandleState Add(CsShipmentSurchargeModel model)
        {
            model.Id = Guid.NewGuid();
            model.ExchangeDate = DateTime.Now;
            model.DatetimeCreated = DateTime.Now;
            var entity = mapper.Map<CsShipmentSurcharge>(model);
            var result = DataContext.Add(entity);
            return result;
        }

        public HandleState DeleteCharge(Guid chargeId)
        {
            var hs = new HandleState();
            try
            {
                var charge = DataContext.Where(x => x.Id == chargeId).FirstOrDefault();
                if (charge == null)
                    hs = new HandleState("Charge not found! ");
                if(charge!=null && (charge.Soano != null || charge.OtherSoa!=null))
                {
                    hs = new HandleState("Cannot delete, this charge is containing Credit Debit Note/SOA no!");
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

        public List<CatPartner> GetAllParnerByJob(Guid JobId)
        {
            try
            {
                List<CatPartner> listPartners = new List<CatPartner>();
                List<Guid> lst_Hbid = ((eFMSDataContext)DataContext.DC).CsTransactionDetail.Where(x => x.JobId == JobId).ToList().Select(x => x.Id).ToList();
                foreach (var id in lst_Hbid)
                {
                    var houseBill = ((eFMSDataContext)DataContext.DC).CsTransactionDetail.Where(x => x.Id == id).FirstOrDefault();
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
                            if (c.ReceiverId != null)
                            {
                                var partner = ((eFMSDataContext)DataContext.DC).CatPartner.Where(x => x.Id == c.ReceiverId).FirstOrDefault();
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

        public List<object> GroupChargeByHB(Guid JobId, string PartnerId,bool getAll=false)
        {
            List<object> returnList = new List<object>();
            List<Guid> lst_Hbid = ((eFMSDataContext)DataContext.DC).CsTransactionDetail.Where(x => x.JobId == JobId).ToList().Select(x => x.Id).ToList();
            foreach (var id in lst_Hbid)
            {
                var houseBill = ((eFMSDataContext)DataContext.DC).CsTransactionDetail.Where(x => x.Id == id).FirstOrDefault();
                List<CsShipmentSurchargeDetailsModel> listCharges = new List<CsShipmentSurchargeDetailsModel>();
                if (houseBill != null)
                {
                    listCharges = Query(houseBill.Id, null);
                    listCharges = listCharges.Where(x => (x.PayerId == PartnerId || x.ReceiverId == PartnerId || x.PaymentObjectId == PartnerId)).ToList();
                }

                //listCharges = getAll==true?listCharges : listCharges.Where(x => (x.Soano == null || x.Soano.Trim()=="")).ToList();
                listCharges = listCharges.Where(x => (x.Soano == null || x.Soano.Trim() == "")).ToList();
              
                foreach(var item in listCharges)
                {
                    var exchangeRate = ((eFMSDataContext)DataContext.DC).CatCurrencyExchange.Where(x => (x.DatetimeCreated.Value.Date == item.ExchangeDate.Value.Date && x.CurrencyFromId==item.CurrencyId && x.CurrencyToId == "VND" && x.Inactive==false)).OrderByDescending(x=>x.DatetimeModified).FirstOrDefault();
                    item.ExchangeRate = exchangeRate?.Rate;
                }
                var returnObj = new { houseBill.Hwbno, houseBill.Hbltype, houseBill.Id, listCharges };

                returnList.Add(returnObj);
                    
            }
            return returnList;

        }

        private List<CsShipmentSurchargeDetailsModel> Query(Guid HbID, string type)
        {
            List<CsShipmentSurchargeDetailsModel> listCharges = new List<CsShipmentSurchargeDetailsModel>();
            var charges = DataContext.Get();
            if (!string.IsNullOrEmpty(type))
            {
                charges = charges.Where(x => x.Type.ToLower() == type.ToLower());
            }
            var query = (from charge in charges
                         where charge.Hblid == HbID 
                         join chargeDetail in ((eFMSDataContext)DataContext.DC).CatCharge on charge.ChargeId equals chargeDetail.Id

                         join partner in ((eFMSDataContext)DataContext.DC).CatPartner on charge.PaymentObjectId equals partner.Id into partnerGroup
                         from p in partnerGroup.DefaultIfEmpty()

                         join receiver in ((eFMSDataContext)DataContext.DC).CatPartner on charge.ReceiverId equals receiver.Id into receiverGroup
                         from r in receiverGroup.DefaultIfEmpty()

                         join payer in ((eFMSDataContext)DataContext.DC).CatPartner on charge.PayerId equals payer.Id into payerGroup
                         from pay in payerGroup.DefaultIfEmpty()

                         join unit in ((eFMSDataContext)DataContext.DC).CatUnit on charge.UnitId equals unit.Id
                         join currency in ((eFMSDataContext)DataContext.DC).CatCurrency on charge.CurrencyId equals currency.Id
                         select new { charge, p, r, pay, unit.UnitNameEn, currency.CurrencyName, chargeDetail.ChargeNameEn, chargeDetail.Code }
                        ).ToList();
            foreach (var item in query)
            {
               
                
                var charge = mapper.Map<CsShipmentSurchargeDetailsModel>(item.charge);
                var exchangeCurrencyToVND = ((eFMSDataContext)DataContext.DC).CatCurrencyExchange.Where(x => (x.DatetimeCreated.Value.Date == charge.ExchangeDate.Value.Date && x.CurrencyFromId == charge.CurrencyId && x.CurrencyToId == "VND" && x.Inactive == false)).OrderByDescending(x => x.DatetimeModified).FirstOrDefault();
                var exchangeUSDToVND = ((eFMSDataContext)DataContext.DC).CatCurrencyExchange.Where(x => (x.DatetimeCreated.Value.Date == charge.ExchangeDate.Value.Date && x.CurrencyFromId == "USD" && x.CurrencyToId == "VND" && x.Inactive == false)).OrderByDescending(x => x.DatetimeModified).FirstOrDefault();
                charge.ExchangeRate = exchangeCurrencyToVND == null ? 1 : exchangeCurrencyToVND.Rate;
                charge.ExchangeRateUSDToVND = exchangeUSDToVND == null ? 1 : exchangeUSDToVND.Rate;
                charge.PartnerName = item.p?.PartnerNameEn;
                charge.NameEn = item?.ChargeNameEn;
                charge.ReceiverName = item.r?.PartnerNameEn;
                charge.PayerName = item.pay?.PartnerNameEn;
                charge.Unit = item.UnitNameEn;
                charge.Currency = item.CurrencyName;
                charge.ChargeCode = item.Code;
                listCharges.Add(charge);
            }
            return listCharges;
        }
    }
}
