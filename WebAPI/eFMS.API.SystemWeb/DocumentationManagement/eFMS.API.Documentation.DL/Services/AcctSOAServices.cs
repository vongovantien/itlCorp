using AutoMapper;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.Service.Models;
using eFMS.API.Documentation.Service.ViewModels;
using ITL.NetCore.Common;
using ITL.NetCore.Connection;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Documentation.DL.Services
{
    public class AcctSOAServices : RepositoryBase<AcctSoa,AcctSOAModel>,IAcctSOAServices
    {
        public AcctSOAServices(IContextBase<AcctSoa> repository,IMapper mapper) : base(repository, mapper)
        {

        }

        private string RandomCode()
        {
            var allowedChars = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ0123456789";
            var head = new char[3];
            var body = new char[4];
            var rd = new Random();
            for (var i = 0; i < 3; i++)
            {
                head[i] = allowedChars[rd.Next(0, allowedChars.Length)];
            }

            for (var i = 0; i < 4; i++)
            {
                body[i] = allowedChars[rd.Next(0, allowedChars.Length)];
            }

            return (new string(head)  +"-"+ new string(body)).ToUpper();
        }

        public HandleState AddNewSOA(AcctSOAModel model)
        {
            try
            {

                var soa = mapper.Map<AcctSoa>(model);
                soa.Id = Guid.NewGuid();
                soa.Code = RandomCode();
                DataContext.Add(soa);

                foreach (var c in model.listShipmentSurcharge)
                {
                    var charge = ((eFMSDataContext)DataContext.DC).CsShipmentSurcharge.Where(x => x.Id == c.Id).FirstOrDefault();
                    if (charge != null)
                    {
                        charge.Soano = soa.Code;
                        charge.Soaclosed = true;
                        charge.DatetimeModified = DateTime.Now;
                        charge.UserModified = "admin";
                    }
                }
                ((eFMSDataContext)DataContext.DC).SaveChanges();

                return new HandleState();
            }
            catch(Exception ex)
            {
                var hs = new HandleState(ex.Message);
                return hs;
            }

        }

        public HandleState UpdateSOA(AcctSOAModel model)
        {
            try
            {
                var soa = DataContext.First(x => (x.Id == model.Id && x.Code == model.Code));
                soa = mapper.Map<AcctSoa>(model);
                if (soa == null)
                {
                    throw new Exception("CD Note not found !");
                }
                var stt = DataContext.Update(soa, x => x.Id == soa.Id);
                if (stt.Success)
                {
                    var chargesOfSOA = ((eFMSDataContext)DataContext.DC).CsShipmentSurcharge.Where(x => x.Soano == soa.Code).ToList();
                    foreach (var item in chargesOfSOA)
                    {
                        item.Soano = null;
                    }
                    foreach (var item in model.listShipmentSurcharge)
                    {
                        var charge = ((eFMSDataContext)DataContext.DC).CsShipmentSurcharge.Where(x => x.Id == item.Id).FirstOrDefault();
                        if (charge != null)
                        {
                            charge.Soano = soa.Code;
                            charge.Soaclosed = true;
                            charge.DatetimeModified = DateTime.Now;
                            charge.UserModified = "admin"; // need update in the future 
                            ((eFMSDataContext)DataContext.DC).CsShipmentSurcharge.Update(charge);
                        }
                    }
                }
                ((eFMSDataContext)DataContext.DC).SaveChanges();

                return new HandleState();

            }
            catch(Exception ex)
            {
                var hs = new HandleState(ex.Message);
                return hs;
            }

        }

        public List<object> GroupSOAByPartner(Guid JobId)
        {
            try
            {
                List<object> returnList = new List<object>();
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
                foreach(var item in listPartners)
                {
                    var SOA = DataContext.Where(x => x.PartnerId == item.Id).ToList();
                    List<object> listSOA = new List<object>();
                    foreach(var soa in SOA)
                    {
                        var chargesOfSOA = ((eFMSDataContext)DataContext.DC).CsShipmentSurcharge.Where(x => x.Soano == soa.Code).ToList();
                        listSOA.Add(new { soa, total_charge= chargesOfSOA.Count });                      

                    }

                    var obj = new { item.PartnerNameEn, item.PartnerNameVn, item.Id, listSOA };
                    if (listSOA.Count > 0)
                    {
                        returnList.Add(obj);
                    }
                   

                }
                return returnList;
              
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private List<CsShipmentSurchargeDetailsModel> Query(Guid HbID, string type)
        {
            List<CsShipmentSurchargeDetailsModel> listCharges = new List<CsShipmentSurchargeDetailsModel>();
            var charges = ((eFMSDataContext)DataContext.DC).CsShipmentSurcharge.Where(x => x.Hblid == HbID);
           
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

        public object GetSOADetails(Guid JobId, string SOACode)
        {
            var Shipment = ((eFMSDataContext)DataContext.DC).CsTransaction.Where(x => x.Id == JobId).FirstOrDefault();
            var Soa = DataContext.Where(x => x.Code == SOACode).FirstOrDefault();
            var partner = ((eFMSDataContext)DataContext.DC).CatPartner.Where(x => x.Id == Soa.PartnerId).FirstOrDefault();

            var pol = ((eFMSDataContext)DataContext.DC).CatPlace.Where(x => x.Id == Shipment.Pol).FirstOrDefault();
            var pod = ((eFMSDataContext)DataContext.DC).CatPlace.Where(x => x.Id == Shipment.Pod).FirstOrDefault();

            if (Shipment==null || Soa == null || partner==null)
            {
                return null;
            }
            
            var charges = ((eFMSDataContext)DataContext.DC).CsShipmentSurcharge.Where(x => x.Soano == SOACode).ToList();

            List<CsTransactionDetail> HBList = new List<CsTransactionDetail>();
            List<CsShipmentSurchargeDetailsModel> listSurcharges = new List<CsShipmentSurchargeDetailsModel>();
            foreach(var item in charges)
            {
                var charge = mapper.Map<CsShipmentSurchargeDetailsModel>(item);               
                var hb = ((eFMSDataContext)DataContext.DC).CsTransactionDetail.Where(x => x.Id == item.Hblid).FirstOrDefault();
                var catCharge = ((eFMSDataContext)DataContext.DC).CatCharge.First(x => x.Id == charge.ChargeId);
                var exchangeRate = ((eFMSDataContext)DataContext.DC).CatCurrencyExchange.Where(x => (x.DatetimeCreated.Value.Date == item.ExchangeDate.Value.Date && x.CurrencyFromId == item.CurrencyId && x.CurrencyToId == "VND" && x.Inactive == false)).OrderByDescending(x => x.DatetimeModified).FirstOrDefault();

                charge.Currency = ((eFMSDataContext)DataContext.DC).CatCurrency.First(x => x.Id == charge.CurrencyId).CurrencyName;
                charge.ExchangeRate = (exchangeRate != null && exchangeRate.Rate != 0) ? exchangeRate.Rate : 1;
                charge.hwbno = hb.Hwbno;
                charge.Unit = ((eFMSDataContext)DataContext.DC).CatUnit.First(x => x.Id == charge.UnitId).UnitNameEn;
                charge.ChargeCode = catCharge.Code;
                charge.NameEn = catCharge.ChargeNameEn;
                listSurcharges.Add(charge);
                HBList.Add(hb);                
                HBList = HBList.Distinct().ToList().OrderBy(x => x.Hwbno).ToList();
            }

            var hbOfLadingNo = string.Empty;
            var mbOfLadingNo = string.Empty;
            var hbConstainers = string.Empty;
            decimal? volum = 0;
            foreach(var item in HBList)
            {
                hbOfLadingNo += (item.Hwbno + ", ");
                mbOfLadingNo += (item.JobNo + ", ");
                var conts = ((eFMSDataContext)DataContext.DC).CsMawbcontainer.Where(x => x.Hblid == item.Id).ToList();
                foreach(var cont in conts)
                {
                    volum += cont.Cbm == null ? 0 : cont.Cbm;
                    var contUnit = ((eFMSDataContext)DataContext.DC).CatUnit.Where(x => x.Id == cont.ContainerTypeId).FirstOrDefault();
                    hbConstainers += (cont.Quantity + "x" + contUnit.UnitNameEn + ",");
                }

            }



            var returnObj = new
            {
                partnerNameEn = partner.PartnerNameEn,
                partnerShippingAddress = partner.AddressShippingEn,
                partnerTel = partner.Tel,
                partnerTaxcode = partner.TaxCode,
                partnerId = partner.Id,
                hbLadingNo = hbOfLadingNo,
                mbLadingNo = mbOfLadingNo,
                jobId = Shipment.Id,
                pol = pol.NameEn,
                polCountry = ((eFMSDataContext)DataContext.DC).CatCountry.Where(x => x.Id == pol.CountryId).FirstOrDefault().NameEn,

                pod = pod.NameEn,
                podCountry = ((eFMSDataContext)DataContext.DC).CatCountry.Where(x => x.Id == pod.CountryId).FirstOrDefault().NameEn,

                vessel = Shipment.FlightVesselName,
                hbConstainers,
                etd= Shipment.Etd,
                eta= Shipment.Eta,
                //soaNo = Soa.Code,
                isLocked = false, // need update later 
                volum,
                listSurcharges,
                Soa
                //charges

            };

            return returnObj;

        }
    }
}
