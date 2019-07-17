using AutoMapper;
using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.DL.Models.ReportResults;
using eFMS.API.Documentation.Service.Contexts;
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

        public List<object> GroupSOAByPartner(Guid id, bool IsHouseBillID)
        {
            try
            {
                List<object> returnList = new List<object>();
                List<CatPartner> listPartners = new List<CatPartner>();

                if(IsHouseBillID == false)
                {
                    List<Guid> lst_Hbid = ((eFMSDataContext)DataContext.DC).CsTransactionDetail.Where(x => x.JobId == id).ToList().Select(x => x.Id).ToList();
                    foreach (var _id in lst_Hbid)
                    {
                        var houseBill = ((eFMSDataContext)DataContext.DC).CsTransactionDetail.Where(x => x.Id == _id).FirstOrDefault();
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
                }
                else
                {
                    List<CsShipmentSurchargeDetailsModel> listCharges = new List<CsShipmentSurchargeDetailsModel>();
                    listCharges = Query(id, null);

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
              
                listPartners = listPartners.Distinct().ToList();
                foreach(var item in listPartners)
                {
                    var jobId = IsHouseBillID == true ? ((eFMSDataContext)DataContext.DC).OpsTransaction.Where(x => x.Hblid == id).FirstOrDefault()?.Id : id;
                    var SOA = DataContext.Where(x => x.PartnerId == item.Id && x.JobId== jobId).ToList();
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

        public AcctSOADetailsModel GetSOADetails(Guid JobId, string SOACode)
        {
            
            var Soa = DataContext.Where(x => x.Code == SOACode).FirstOrDefault();
            var partner = ((eFMSDataContext)DataContext.DC).CatPartner.Where(x => x.Id == Soa.PartnerId).FirstOrDefault();

            CatPlace pol = new CatPlace();
            CatPlace pod = new CatPlace();

            var Shipment1 = ((eFMSDataContext)DataContext.DC).CsTransaction.Where(x => x.Id == JobId).FirstOrDefault();
            var Shipment2 = ((eFMSDataContext)DataContext.DC).OpsTransaction.Where(x => x.Id == JobId).FirstOrDefault();

            if(Shipment1 != null)
            {
                pol = ((eFMSDataContext)DataContext.DC).CatPlace.Where(x => x.Id == Shipment1.Pol).FirstOrDefault();
                pod = ((eFMSDataContext)DataContext.DC).CatPlace.Where(x => x.Id == Shipment1.Pod).FirstOrDefault();
            }
            else
            {
                pol = ((eFMSDataContext)DataContext.DC).CatPlace.Where(x => x.Id == Shipment2.Pol).FirstOrDefault();
                pod = ((eFMSDataContext)DataContext.DC).CatPlace.Where(x => x.Id == Shipment2.Pod).FirstOrDefault();
            }

            

            if ((Shipment1==null && Shipment2==null) || Soa == null || partner==null)
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
                charge.hwbno = hb!=null ? hb.Hwbno : Shipment2.Hwbno;
                charge.Unit = ((eFMSDataContext)DataContext.DC).CatUnit.First(x => x.Id == charge.UnitId).UnitNameEn;
                charge.ChargeCode = catCharge.Code;
                charge.NameEn = catCharge.ChargeNameEn;
                listSurcharges.Add(charge);
                if (hb != null)
                {
                    HBList.Add(hb);
                }
                     
                HBList = HBList.Distinct().ToList().OrderBy(x => x?.Hwbno).ToList();
            }

            var hbOfLadingNo = Shipment2!=null? Shipment2.Hwbno: string.Empty;
            var mbOfLadingNo = Shipment2!=null? Shipment2.JobNo: string.Empty;
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

            var countries = ((eFMSDataContext)DataContext.DC).CatCountry.ToList();



            //var returnObj = new
            //{
            //    partnerNameEn = partner?.PartnerNameEn,
            //    partnerShippingAddress = partner?.AddressShippingEn,
            //    partnerTel = partner?.Tel,
            //    partnerTaxcode = partner?.TaxCode,
            //    partnerId = partner?.Id,
            //    hbLadingNo = hbOfLadingNo,
            //    mbLadingNo = mbOfLadingNo,
            //    jobId = Shipment1 != null? Shipment1.Id:Shipment2.Id, 
            //    pol = pol?.NameEn,
            //    polCountry = pol==null?null:countries.Where(x => x.Id == pol.CountryId).FirstOrDefault()?.NameEn,

            //    pod = pod?.NameEn,
            //    podCountry = pod==null?null:countries.Where(x => x.Id == pod.CountryId).FirstOrDefault()?.NameEn,

            //    vessel = Shipment1!=null?Shipment1.FlightVesselName:Shipment2.FlightVessel, 
            //    hbConstainers,
            //    etd= Shipment1!=null?Shipment1.Etd:Shipment2.ServiceDate,  //Shipment?.Etd,
            //    eta= Shipment1!=null?Shipment1.Eta:Shipment2.FinishDate, //Shipment?.Eta,
            //    //soaNo = Soa.Code,
            //    isLocked = false, // need update later 
            //    volum,
            //    listSurcharges,
            //    Soa
            //    //charges

            //};

            AcctSOADetailsModel SoaDetails = new AcctSOADetailsModel();
            SoaDetails.PartnerNameEn = partner?.PartnerNameEn;
            SoaDetails.PartnerShippingAddress = partner?.AddressShippingEn;
            SoaDetails.PartnerTel = partner?.Tel;
            SoaDetails.PartnerTaxcode = partner?.TaxCode;
            SoaDetails.PartnerId = partner?.Id;
            SoaDetails.HbLadingNo = hbOfLadingNo;
            SoaDetails.MbLadingNo = mbOfLadingNo;
            SoaDetails.JobId = Shipment1 != null ? Shipment1.Id : Shipment2.Id;
            SoaDetails.Pol = pol?.NameEn;
            SoaDetails.PolCountry = pol == null ? null : countries.Where(x => x.Id == pol.CountryId).FirstOrDefault()?.NameEn;
            SoaDetails.Pod = pod?.NameEn;
            SoaDetails.PodCountry = pod == null ? null : countries.Where(x => x.Id == pod.CountryId).FirstOrDefault()?.NameEn;
            SoaDetails.Vessel = Shipment1 != null ? Shipment1.FlightVesselName : Shipment2.FlightVessel;
            SoaDetails.HbConstainers = hbConstainers;
            SoaDetails.Etd = Shipment1 != null ? Shipment1.Etd : Shipment2.ServiceDate;
            SoaDetails.Eta = Shipment1 != null ? Shipment1.Eta : Shipment2.FinishDate;
            SoaDetails.IsLocked = false;
            SoaDetails.Volum = volum;
            SoaDetails.ListSurcharges = listSurcharges;
            SoaDetails.Soa = Soa;


            return SoaDetails;

        }

        public HandleState DeleteSOA(Guid idSoA)
        {
            var hs = new HandleState();
            try
            {
                var cdNote = DataContext.Where(x => x.Id == idSoA).FirstOrDefault();
                if (cdNote == null)
                {
                    hs = new HandleState("Credit debit note not found !");
                }
                else
                {
                    var charges = ((eFMSDataContext)DataContext.DC).CsShipmentSurcharge.Where(x => x.Cdno == cdNote.Code).ToList();
                    var isOtherSOA = false;
                    foreach(var item in charges)
                    {
                        if (item.Soano != null)
                        {
                            isOtherSOA = true;
                        }
                    }
                    if (isOtherSOA == true)
                    {
                        hs = new HandleState("Cannot delete, this credit debit not is containing at least one charge have SOA no!");
                    }
                    else
                    {                       
                        var _hs = DataContext.Delete(x => x.Id == idSoA);
                        if (hs.Success)
                        {
                            foreach (var item in charges)
                            {
                                item.Soano = null;
                                item.UserModified = cdNote.UserModified;
                                item.DatetimeModified = DateTime.Now;
                                ((eFMSDataContext)DataContext.DC).CsShipmentSurcharge.Update(item);
                            }
                            ((eFMSDataContext)DataContext.DC).SaveChanges();

                        }
                        
                    }
                }

            }
            catch (Exception ex)
            {
                hs = new HandleState(ex.Message);
            }
            return hs;
        }

        public Crystal Preview(AcctSOADetailsModel model)
        {
            if (model == null)
            {
                return null;
            }
            Crystal result = null;
            var parameter = new AcctSOAReportParams
            {
               DBTitle = "N/A",
               DebitNo = model.Soa.Code,
               TotalDebit = model.TotalDebit==null? "N/A": model.TotalDebit.ToString(),
               TotalCredit = model.TotalCredit == null ? "N/A" : model.TotalCredit.ToString(),
               DueToTitle = "N/A",
               DueTo = "N/A",
               DueToCredit = "N/A",
               SayWordAll = "N/A",
               CompanyName = "N/A",
               CompanyAddress1 = "N/A",
               ComapnyAddress2 = "N/A",
               CompanyDescription = "N/A",
               Website = "efms.itlvn.com",
               IbanCode = "N/A",
               AccountName = "N/A",
               BankName = "N/A",
               SwiftAccs = "N/A",
               AccsUSD = "N/A",
               AccsVND = "N/A",
               BankAddress = "N/A",
               Paymentterms = "N/A",
               DecimalNo = 2,
               CurrDecimal = 2,
               IssueInv = "N/A",
               InvoiceInfo = "N/A",
               Contact = "N/A",
               IssueDate = DateTime.Now.ToString(),
               OtherRef = "N/A"
            };
            var listSOA = new List<AcctSOAReport>();
            if (model.ListSurcharges.Count > 0)
            {
                foreach(var item in model.ListSurcharges)
                {
                    var acctsoa = new AcctSOAReport
                    {
                        SortIndex = null,
                        Subject = null,
                        PartnerID = model.PartnerId,
                        PartnerName = model.PartnerNameEn,
                        PersonalContact = "N/A",
                        Address = model.PartnerShippingAddress,
                        Taxcode = model.PartnerTaxcode,
                        Workphone = model.PartnerTel,
                        Fax =  "N/A",
                        TransID = "N/A",
                        LoadingDate = null,
                        Commodity = "N/A",
                        PortofLading = model.PolName,
                        PortofUnlading = model.PodName,
                        MAWB = model.MbLadingNo,
                        Invoice = model.Soa.InvoiceNo,
                        EstimatedVessel = "N/A",
                        ArrivalDate = null,
                        Noofpieces = null,
                        Delivery = null,
                        HWBNO = model.HbLadingNo,
                        Description = "N/A",
                        Quanity = null,
                        QUnit = "N/A",
                        UnitPrice = null,
                        VAT = null,
                        Debit = model.TotalDebit,
                        Credit = model.TotalCredit,
                        Notes = "N/A",
                        InputData = "N/A",
                        PONo = "N/A",
                        TransNotes = "N/A",
                        Shipper = "N/A",
                        Consignee = "N/A",
                        ContQty = model.HbConstainers,
                        ContSealNo = "N/A",
                        Deposit = null,
                        DepositCurr = "N/A",
                        DecimalSymbol = "N/A",
                        DigitSymbol = "N/A",
                        DecimalNo = null,
                        CurrDecimalNo = null,
                        VATInvoiceNo = "N/A",
                        GW = null,
                        NW = null,
                        SeaCBM = null,
                        SOTK = "N/A",
                        NgayDK = null,
                        Cuakhau = "N/A",
                        DeliveryPlace = null,
                        TransDate = null,
                        Unit = "N/A",
                        UnitPieaces = "N/A"                                                             

                    };
                    listSOA.Add(acctsoa);
                }
            }
            else
            {
                return null;
            }

            result = new Crystal
            {
                ReportName = "LogisticsDebitNewDNTT.rpt",
                AllowPrint = true,
                AllowExport = true
            };
            result.AddDataSource(listSOA);
            result.FormatType = ExportFormatType.PortableDocFormat;
            result.SetParameter(parameter);
            return result;
                
        }
    }
}
