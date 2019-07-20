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
    public class AcctCDNoteServices : RepositoryBase<AcctCdnote, AcctCdnoteModel>, IAcctCDNoteServices
    {
        public AcctCDNoteServices(IContextBase<AcctCdnote> repository,IMapper mapper) : base(repository, mapper)
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

        public HandleState AddNewCDNote(AcctCdnoteModel model)
        {
            try
            {

                var cdNote = mapper.Map<AcctCdnote>(model);
                cdNote.Id = Guid.NewGuid();
                cdNote.Code = RandomCode();
                DataContext.Add(cdNote);

                foreach (var c in model.listShipmentSurcharge)
                {
                    var charge = ((eFMSDataContext)DataContext.DC).CsShipmentSurcharge.Where(x => x.Id == c.Id).FirstOrDefault();
                    if (charge != null)
                    {
                        charge.Cdno = cdNote.Code;
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

        public HandleState UpdateCDNote(AcctCdnoteModel model)
        {
            try
            {
                var cdNote = DataContext.First(x => (x.Id == model.Id && x.Code == model.Code));
                cdNote = mapper.Map<AcctCdnote>(model);
                if (cdNote == null)
                {
                    throw new Exception("CD Note not found !");
                }
                var stt = DataContext.Update(cdNote, x => x.Id == cdNote.Id);
                if (stt.Success)
                {
                    var chargesOfSOA = ((eFMSDataContext)DataContext.DC).CsShipmentSurcharge.Where(x => x.Cdno == cdNote.Code).ToList();
                    foreach (var item in chargesOfSOA)
                    {
                        item.Cdno = null;
                    }
                    foreach (var item in model.listShipmentSurcharge)
                    {
                        var charge = ((eFMSDataContext)DataContext.DC).CsShipmentSurcharge.Where(x => x.Id == item.Id).FirstOrDefault();
                        if (charge != null)
                        {
                            charge.Cdno = cdNote.Code;
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

        public List<object> GroupCDNoteByPartner(Guid id, bool IsHouseBillID)
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
                    var cdNotes = DataContext.Where(x => x.PartnerId == item.Id && x.JobId== jobId).ToList();
                    List<object> listCDNote = new List<object>();
                    foreach(var cdNote in cdNotes)
                    {
                        var chargesOfCDNote = ((eFMSDataContext)DataContext.DC).CsShipmentSurcharge.Where(x => x.Cdno == cdNote.Code).ToList();
                        listCDNote.Add(new { cdNote, total_charge= chargesOfCDNote.Count });                      

                    }

                    var obj = new { item.PartnerNameEn, item.PartnerNameVn, item.Id, listCDNote };
                    if (listCDNote.Count > 0)
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

        public AcctCDNoteDetailsModel GetCDNoteDetails(Guid JobId, string CDNoteCode)
        {
            
            var cdNote = DataContext.Where(x => x.Code == CDNoteCode).FirstOrDefault();
            var partner = ((eFMSDataContext)DataContext.DC).CatPartner.FirstOrDefault(x => x.Id == cdNote.PartnerId);

            CatPlace pol = new CatPlace();
            CatPlace pod = new CatPlace();

            var transaction = ((eFMSDataContext)DataContext.DC).CsTransaction.FirstOrDefault(x => x.Id == JobId);
            var opsTansaction = ((eFMSDataContext)DataContext.DC).OpsTransaction.FirstOrDefault(x => x.Id == JobId);

            if(transaction != null)
            {
                pol = ((eFMSDataContext)DataContext.DC).CatPlace.FirstOrDefault(x => x.Id == transaction.Pol);
                pod = ((eFMSDataContext)DataContext.DC).CatPlace.FirstOrDefault(x => x.Id == transaction.Pod);
            }
            else
            {
                pol = ((eFMSDataContext)DataContext.DC).CatPlace.FirstOrDefault(x => x.Id == opsTansaction.Pol);
                pod = ((eFMSDataContext)DataContext.DC).CatPlace.FirstOrDefault(x => x.Id == opsTansaction.Pod);
            }

            

            if ((transaction == null && opsTansaction == null) || cdNote == null || partner==null)
            {
                return null;
            }
            
            var charges = ((eFMSDataContext)DataContext.DC).CsShipmentSurcharge.Where(x => x.Cdno == CDNoteCode).ToList();

            List<CsTransactionDetail> HBList = new List<CsTransactionDetail>();
            List<CsShipmentSurchargeDetailsModel> listSurcharges = new List<CsShipmentSurchargeDetailsModel>();
            foreach(var item in charges)
            {
                var charge = mapper.Map<CsShipmentSurchargeDetailsModel>(item);               
                var hb = ((eFMSDataContext)DataContext.DC).CsTransactionDetail.FirstOrDefault(x => x.Id == item.Hblid);
                var catCharge = ((eFMSDataContext)DataContext.DC).CatCharge.First(x => x.Id == charge.ChargeId);
                var exchangeRate = ((eFMSDataContext)DataContext.DC).CatCurrencyExchange.Where(x => (x.DatetimeCreated.Value.Date == item.ExchangeDate.Value.Date && x.CurrencyFromId == item.CurrencyId && x.CurrencyToId == "VND" && x.Inactive == false)).OrderByDescending(x => x.DatetimeModified).FirstOrDefault();

                charge.Currency = ((eFMSDataContext)DataContext.DC).CatCurrency.First(x => x.Id == charge.CurrencyId).CurrencyName;
                charge.ExchangeRate = (exchangeRate != null && exchangeRate.Rate != 0) ? exchangeRate.Rate : 1;
                charge.hwbno = hb!=null ? hb.Hwbno : opsTansaction.Hwbno;
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
            var hbOfLadingNo = string.Empty;
            var mbOfLadingNo = string.Empty;
            if (transaction != null)
            {
                hbOfLadingNo = transaction?.Mawb;
            }
            else
            {
                hbOfLadingNo = opsTansaction?.Hwbno;
                mbOfLadingNo = opsTansaction?.Mblno;
            }
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
            AcctCDNoteDetailsModel soaDetails = new AcctCDNoteDetailsModel();
            soaDetails.PartnerNameEn = partner?.PartnerNameEn;
            soaDetails.PartnerShippingAddress = partner?.AddressShippingEn;
            soaDetails.PartnerTel = partner?.Tel;
            soaDetails.PartnerTaxcode = partner?.TaxCode;
            soaDetails.PartnerId = partner?.Id;
            soaDetails.HbLadingNo = hbOfLadingNo;
            soaDetails.MbLadingNo = mbOfLadingNo;
            soaDetails.JobId = transaction != null ? transaction.Id : opsTansaction.Id;
            soaDetails.JobNo = transaction != null ? transaction.JobNo : opsTansaction.JobNo;
            soaDetails.Pol = pol?.NameEn;
            if (soaDetails.PolCountry != null)
            {
                soaDetails.PolCountry = pol == null ? null : countries.FirstOrDefault(x => x.Id == pol.CountryId)?.NameEn;
            }
            soaDetails.Pod = pod?.NameEn;
            if (countries != null)
            {
                soaDetails.PodCountry = pod == null ? null : countries.FirstOrDefault(x => x.Id == pod.CountryId)?.NameEn;
            }
            soaDetails.Vessel = transaction != null ? transaction.FlightVesselName : opsTansaction.FlightVessel;
            soaDetails.HbConstainers = hbConstainers;
            soaDetails.Etd = transaction != null ? transaction.Etd : opsTansaction.ServiceDate;
            soaDetails.Eta = transaction != null ? transaction.Eta : opsTansaction.FinishDate;
            soaDetails.IsLocked = false;
            soaDetails.Volum = volum;
            soaDetails.ListSurcharges = listSurcharges;
            soaDetails.CDNote = cdNote;
            soaDetails.ProductService = opsTansaction.ProductService;
            soaDetails.ServiceMode = opsTansaction.ServiceMode;



            return soaDetails;

        }

        public HandleState DeleteCDNote(Guid idSoA)
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
                                item.Cdno = null;
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

        public Crystal Preview(AcctCDNoteDetailsModel model)
        {
            if (model == null)
            {
                return null;
            }
            Crystal result = null;
            var parameter = new AcctSOAReportParams
            {
               DBTitle = "N/A",
               DebitNo = model.CDNote.Code,
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
                        Invoice = model.CDNote.InvoiceNo,
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
