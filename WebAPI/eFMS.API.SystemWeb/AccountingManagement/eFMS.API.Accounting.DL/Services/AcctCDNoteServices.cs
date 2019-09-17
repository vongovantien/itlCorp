using AutoMapper;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.ReportResults;
using eFMS.API.Accounting.Service.Contexts;
using eFMS.API.Accounting.Service.Models;
using eFMS.API.Common.Globals;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Accounting.DL.Services
{
    public class AcctCDNoteServices : RepositoryBase<AcctCdnote, AcctCdnoteModel>, IAcctCDNoteServices
    {
        private readonly ICurrentUser currentUser;
        public AcctCDNoteServices(IContextBase<AcctCdnote> repository,IMapper mapper, ICurrentUser user) : base(repository, mapper)
        {
            currentUser = user;
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
                var hs = DataContext.Add(cdNote);

                if (hs.Success)
                {
                    foreach (var c in model.listShipmentSurcharge)
                    {
                        var charge = ((eFMSDataContext)DataContext.DC).CsShipmentSurcharge.Where(x => x.Id == c.Id).FirstOrDefault();
                        if (charge != null)
                        {
                            if(charge.Type == "BUY")
                            {
                                charge.CreditNo = cdNote.Code;
                            }
                            else if(charge.Type == "SELL")
                            {
                                charge.DebitNo = cdNote.Code;
                            }
                            else
                            {
                                if(model.PartnerId == charge.PaymentObjectId)
                                {
                                    charge.DebitNo = cdNote.Code;
                                }
                                if(model.PartnerId == charge.PayerId)
                                {
                                    charge.CreditNo = cdNote.Code;
                                }
                            }
                            //charge.Cdno = cdNote.Code; -- to continue
                            //charge.Soaclosed = true;
                            charge.DatetimeModified = DateTime.Now;
                            charge.UserModified = currentUser.UserID;
                        }
                        ((eFMSDataContext)DataContext.DC).CsShipmentSurcharge.Update(charge);
                    }
                    var jobOpsTrans = ((eFMSDataContext)DataContext.DC).OpsTransaction.FirstOrDefault();
                    if (jobOpsTrans != null)
                    {
                        jobOpsTrans.UserModified = model.UserModified;
                        jobOpsTrans.ModifiedDate = DateTime.Now;
                        ((eFMSDataContext)DataContext.DC).OpsTransaction.Update(jobOpsTrans);
                    }
                    var jobCSTrans = ((eFMSDataContext)DataContext.DC).CsTransaction.FirstOrDefault();
                    if (jobCSTrans != null)
                    {
                        jobCSTrans.UserModified = model.UserModified;
                        jobCSTrans.ModifiedDate = DateTime.Now;
                        ((eFMSDataContext)DataContext.DC).CsTransaction.Update(jobCSTrans);
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
                    foreach (var item in model.listShipmentSurcharge)
                    {
                        item.Cdclosed = true;
                        item.DatetimeModified = DateTime.Now;
                        item.UserModified = currentUser.UserID; // need update in the future 
                        ((eFMSDataContext)DataContext.DC).CsShipmentSurcharge.Update(item);
                    }
                    var jobOpsTrans = ((eFMSDataContext)DataContext.DC).OpsTransaction.FirstOrDefault();
                    if(jobOpsTrans != null)
                    {
                        jobOpsTrans.UserModified = model.UserModified;
                        jobOpsTrans.ModifiedDate = DateTime.Now;
                        ((eFMSDataContext)DataContext.DC).OpsTransaction.Update(jobOpsTrans);
                    }
                    var jobCSTrans = ((eFMSDataContext)DataContext.DC).CsTransaction.FirstOrDefault();
                    if(jobCSTrans != null)
                    {
                        jobCSTrans.UserModified = model.UserModified;
                        jobCSTrans.ModifiedDate = DateTime.Now;
                        ((eFMSDataContext)DataContext.DC).CsTransaction.Update(jobCSTrans);
                    }
                    ((eFMSDataContext)DataContext.DC).SaveChanges();
                }

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
                        // -to continue
                        //var chargesOfCDNote = ((eFMSDataContext)DataContext.DC).CsShipmentSurcharge.Where(x => x.Cdno == cdNote.Code).ToList();
                        var chargesOfCDNote = ((eFMSDataContext)DataContext.DC).CsShipmentSurcharge.Where(x => x.CreditNo == cdNote.Code || x.DebitNo == cdNote.Code).ToList();
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

                         join payer in ((eFMSDataContext)DataContext.DC).CatPartner on charge.PayerId equals payer.Id into payerGroup
                         from pay in payerGroup.DefaultIfEmpty()

                         join unit in ((eFMSDataContext)DataContext.DC).CatUnit on charge.UnitId equals unit.Id
                         join currency in ((eFMSDataContext)DataContext.DC).CatCurrency on charge.CurrencyId equals currency.Id
                         select new { charge, p, pay, unit.UnitNameEn, currency.CurrencyName, chargeDetail.ChargeNameEn, chargeDetail.Code }
                        ).ToList();
            foreach (var item in query)
            {
                var charge = mapper.Map<CsShipmentSurchargeDetailsModel>(item.charge);
                charge.PartnerName = item.p?.PartnerNameEn;
                charge.NameEn = item?.ChargeNameEn;
                if(charge.Type == "OBH")
                {
                    charge.ReceiverName = item.p?.PartnerNameEn;
                }
                charge.PayerName = item.pay?.PartnerNameEn;
                charge.Unit = item.UnitNameEn;
                charge.Currency = item.CurrencyName;
                charge.ChargeCode = item.Code;
                listCharges.Add(charge);
            }
            return listCharges;
        }

        public AcctCDNoteDetailsModel GetCDNoteDetails(Guid JobId, string cdNo)
        {
            AcctCDNoteDetailsModel soaDetails = new AcctCDNoteDetailsModel();
            var cdNote = DataContext.Where(x => x.Code == cdNo).FirstOrDefault();
            var partner = ((eFMSDataContext)DataContext.DC).CatPartner.FirstOrDefault(x => x.Id == cdNote.PartnerId);

            CatPlace pol = new CatPlace();
            CatPlace pod = new CatPlace();

            var transaction = ((eFMSDataContext)DataContext.DC).CsTransaction.FirstOrDefault(x => x.Id == JobId);
            var opsTansaction = ((eFMSDataContext)DataContext.DC).OpsTransaction.FirstOrDefault(x => x.Id == JobId);
            string warehouseId = null;

            if (transaction != null)
            {
                pol = ((eFMSDataContext)DataContext.DC).CatPlace.FirstOrDefault(x => x.Id == transaction.Pol);
                pod = ((eFMSDataContext)DataContext.DC).CatPlace.FirstOrDefault(x => x.Id == transaction.Pod);
                warehouseId = transaction.WareHouseId ?? null;
            }
            else
            {
                pol = ((eFMSDataContext)DataContext.DC).CatPlace.FirstOrDefault(x => x.Id == opsTansaction.Pol);
                pod = ((eFMSDataContext)DataContext.DC).CatPlace.FirstOrDefault(x => x.Id == opsTansaction.Pod);
                warehouseId = opsTansaction.WarehouseId?.ToString();
            }

            if ((transaction == null && opsTansaction == null) || cdNote == null || partner==null)
            {
                return null;
            }
            //to continue
            //var charges = ((eFMSDataContext)DataContext.DC).CsShipmentSurcharge.Where(x => x.Cdno == CDNoteCode).ToList();
            var charges = ((eFMSDataContext)DataContext.DC).CsShipmentSurcharge.Where(x => x.CreditNo == cdNo || x.DebitNo == cdNo).ToList();

            List<CsTransactionDetail> HBList = new List<CsTransactionDetail>();
            List<CsShipmentSurchargeDetailsModel> listSurcharges = new List<CsShipmentSurchargeDetailsModel>();
            if(warehouseId != null)
            {
                soaDetails.WarehouseName = ((eFMSDataContext)DataContext.DC).CatPlace.FirstOrDefault(x => x.Id == new Guid(warehouseId))?.NameEn;
            }
            soaDetails.CreatedDate = ((DateTime)cdNote.DatetimeCreated).ToString("dd'/'MM'/'yyyy");
            foreach (var item in charges)
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
                soaDetails.CBM = opsTansaction.SumCbm;
                soaDetails.GW = opsTansaction.SumGrossWeight;
                soaDetails.ServiceDate = opsTansaction.ServiceDate;
                soaDetails.HbLadingNo = opsTansaction?.Hwbno;
                soaDetails.MbLadingNo = opsTansaction?.Mblno;
                soaDetails.SumContainers = opsTansaction.SumContainers;
                soaDetails.SumPackages = opsTansaction.SumPackages;
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
            soaDetails.PartnerNameEn = partner?.PartnerNameEn;
            soaDetails.PartnerShippingAddress = partner?.AddressShippingEn;
            soaDetails.PartnerTel = partner?.Tel;
            soaDetails.PartnerTaxcode = partner?.TaxCode;
            soaDetails.PartnerId = partner?.Id;
            soaDetails.PartnerFax = partner?.Fax;
            soaDetails.PartnerPersonalContact = partner.ContactPerson;
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
                    //to continue
                    //var charges = ((eFMSDataContext)DataContext.DC).CsShipmentSurcharge.Where(x => x.Cdno == cdNote.Code).ToList();
                    var charges = ((eFMSDataContext)DataContext.DC).CsShipmentSurcharge.Where(x => x.CreditNo == cdNote.Code || x.DebitNo == cdNote.Code).ToList();
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
                                //-to contiue
                                //item.Cdno = null;
                                if(item.Type == "BUY")
                                {
                                    item.CreditNo = null;
                                }
                                else if(item.Type == "BUY")
                                {
                                    item.DebitNo = null;
                                }
                                else
                                {
                                    if(item.DebitNo == cdNote.Code)
                                    {
                                        item.DebitNo = null;
                                    }
                                    if(item.CreditNo == cdNote.Code)
                                    {
                                        item.CreditNo = null;
                                    }
                                }
                                item.UserModified = cdNote.UserModified;
                                item.DatetimeModified = DateTime.Now;
                                ((eFMSDataContext)DataContext.DC).CsShipmentSurcharge.Update(item);
                            }
                            ((eFMSDataContext)DataContext.DC).SaveChanges();
                            var jobOpsTrans = ((eFMSDataContext)DataContext.DC).OpsTransaction.FirstOrDefault();
                            if (jobOpsTrans != null)
                            {
                                //jobOpsTrans.UserModified = model.UserModified;
                                jobOpsTrans.ModifiedDate = DateTime.Now;
                                ((eFMSDataContext)DataContext.DC).OpsTransaction.Update(jobOpsTrans);
                            }
                            var jobCSTrans = ((eFMSDataContext)DataContext.DC).CsTransaction.FirstOrDefault();
                            if (jobCSTrans != null)
                            {
                                //jobCSTrans.UserModified = model.UserModified;
                                jobCSTrans.ModifiedDate = DateTime.Now;
                                ((eFMSDataContext)DataContext.DC).CsTransaction.Update(jobCSTrans);
                            }
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
               TotalDebit = model.TotalDebit==null? string.Empty: model.TotalDebit.ToString(),
               TotalCredit = model.TotalCredit == null ? string.Empty: model.TotalCredit.ToString(),
               DueToTitle = "N/A",
               DueTo = "N/A",
               DueToCredit = "N/A",
               SayWordAll = "N/A",
               CompanyName = "N/A",
               CompanyAddress1 = "N/A",
               CompanyAddress2 = "N/A",
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
               IssuedDate = model.CreatedDate,
               OtherRef = "N/A"
            };
            string trans = string.Empty;
            string port = string.Empty;
            if (model.ServiceMode == "Export")
            {
                trans = "X";
                port = model.Pol;
            }
            else
            {
                port = model.Pod;
            }
            var listSOA = new List<AcctSOAReport>();
            if (model.ListSurcharges.Count > 0)
            {
                foreach(var item in model.ListSurcharges)
                {
                    string subject = string.Empty;
                    if(item.Type == "OBH")
                    {
                        subject = "ON BEHALF";
                    }
                    var acctCDNo = new AcctSOAReport
                    {
                        SortIndex = null,
                        Subject = subject,
                        PartnerID = model.PartnerId,
                        PartnerName = model.PartnerNameEn,
                        PersonalContact = model.PartnerPersonalContact,
                        Address = model.PartnerShippingAddress,
                        Taxcode = model.PartnerTaxcode,
                        Workphone = model.PartnerTel,
                        Fax =  model.PartnerFax,
                        TransID = trans,
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
                        Description = item.NameEn,
                        Quantity = item.Quantity,
                        QUnit = "N/A",
                        UnitPrice = item.UnitPrice,
                        VAT = null,
                        Debit = model.TotalDebit,
                        Credit = model.TotalCredit,
                        Notes = item.Notes,
                        InputData = "N/A",
                        PONo = string.Empty,
                        TransNotes = "N/A",
                        Shipper = model.PartnerNameEn,
                        Consignee = model.PartnerNameEn,
                        ContQty = model.HbConstainers,
                        ContSealNo = "N/A",
                        Deposit = null,
                        DepositCurr = "N/A",
                        DecimalSymbol = "N/A",
                        DigitSymbol = "N/A",
                        DecimalNo = null,
                        CurrDecimalNo = null,
                        VATInvoiceNo = item.InvoiceNo,
                        GW = null,
                        NW = null,
                        SeaCBM = null,
                        SOTK = "N/A",
                        NgayDK = null,
                        Cuakhau = port,
                        DeliveryPlace = model.WarehouseName,
                        TransDate = null,
                        Unit = item.CurrencyId,
                        UnitPieaces = "N/A"              
                    };
                    listSOA.Add(acctCDNo);
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
