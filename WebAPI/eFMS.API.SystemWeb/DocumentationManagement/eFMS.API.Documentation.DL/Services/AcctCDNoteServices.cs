using AutoMapper;
using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.DL.Models.ReportResults;
using eFMS.API.Documentation.Service.Contexts;
using eFMS.API.Documentation.Service.Models;
using eFMS.API.Documentation.Service.ViewModels;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eFMS.API.Documentation.DL.Services
{
    public class AcctCDNoteServices : RepositoryBase<AcctCdnote, AcctCdnoteModel>, IAcctCDNoteServices
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICurrentUser currentUser;
        IContextBase<CsShipmentSurcharge> surchargeRepository;
        IContextBase<OpsTransaction> opstransRepository;
        IContextBase<CsTransaction> cstransRepository;
        IContextBase<CatPartner> partnerRepositoty;
        IContextBase<CsTransactionDetail> trandetailRepositoty;
        IContextBase<CatCharge> catchargeRepository;
        IContextBase<CatCurrency> currencyRepository;
        IContextBase<CatUnit> unitRepository;
        IContextBase<CatPlace> placeRepository;
        ICsShipmentSurchargeService surchargeService;

        public AcctCDNoteServices(IStringLocalizer<LanguageSub> localizer,
            IContextBase<AcctCdnote> repository,IMapper mapper, ICurrentUser user, 
            IContextBase<CsShipmentSurcharge> surchargeRepo,
            IContextBase<OpsTransaction> opstransRepo,
            IContextBase<CsTransaction> cstransRepo,
            IContextBase<CatPartner> partnerRepo,
            IContextBase<CsTransactionDetail> trandetailRepo,
            IContextBase<CatCharge> catchargeRepo,
            IContextBase<CatCurrency> currencyRepo,
            IContextBase<CatUnit> unitRepo,
            IContextBase<CatPlace> placeRepo,
            ICsShipmentSurchargeService surcharge) : base(repository, mapper)
        {
            stringLocalizer = localizer;
            currentUser = user;
            surchargeRepository = surchargeRepo;
            opstransRepository = opstransRepo;
            cstransRepository = cstransRepo;
            partnerRepositoty = partnerRepo;
            trandetailRepositoty = trandetailRepo;
            catchargeRepository = catchargeRepo;
            currencyRepository = currencyRepo;
            unitRepository = unitRepo;
            placeRepository = placeRepo;
            surchargeService = surcharge;
        }

        private string CreateCode(string typeCDNote)
        {
            string code = "LG";
            switch (typeCDNote)
            {
                case "CREDIT":
                    code = code + "CN";
                    break;
                case "DEBIT":
                    code = code + "DN";
                    break;
                case "INVOICE":
                    code = code + "IV";
                    break;
            }
            int count = 0;
            var cdCode = DataContext.Get(x => x.DatetimeCreated.Value.Month == DateTime.Now.Month && x.DatetimeCreated.Value.Year == DateTime.Now.Year)
                .OrderByDescending(x => x.DatetimeModified).FirstOrDefault()?.Code;
            if(cdCode != null)
            {
                cdCode = cdCode.Substring(8, 5);
                Int32.TryParse(cdCode, out count);
            }
            code = GenerateID.GenerateCDNoteNo(code, count);
            return code;
        }

        public HandleState AddNewCDNote(AcctCdnoteModel model)
        {
            try
            {
                model.Id = Guid.NewGuid();
                model.Code = CreateCode(model.Type);
                model.UserCreated = currentUser.UserID;
                model.DatetimeCreated = DateTime.Now;
                var hs = Add(model, false);

                if (hs.Success)
                {
                    foreach (var c in model.listShipmentSurcharge)
                    {
                        var charge = surchargeRepository.Get(x => x.Id == c.Id).FirstOrDefault();
                        if (charge != null)
                        {
                            if(charge.Type == "BUY")
                            {
                                charge.CreditNo = model.Code;
                            }
                            else if(charge.Type == "SELL")
                            {
                                charge.DebitNo = model.Code;
                            }
                            else
                            {
                                if(model.PartnerId == charge.PaymentObjectId)
                                {
                                    charge.DebitNo = model.Code;
                                }
                                if(model.PartnerId == charge.PayerId)
                                {
                                    charge.CreditNo = model.Code;
                                }
                            }
                            charge.DatetimeModified = DateTime.Now;
                            charge.UserModified = currentUser.UserID;
                        }
                        surchargeRepository.Update(charge, x => x.Id == charge.Id, false);
                    }
                }
                UpdateJobModifyTime(model.JobId);
                SubmitChanges();
                surchargeRepository.SubmitChanges();
                opstransRepository.SubmitChanges();
                cstransRepository.SubmitChanges();

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
                if (cdNote == null)
                {
                    return new HandleState(stringLocalizer[LanguageSub.MSG_CDNOTE_NOT_NOT_FOUND].Value);
                }
                cdNote = mapper.Map<AcctCdnote>(model);
                var stt = DataContext.Update(cdNote, x => x.Id == cdNote.Id, false);
                if (stt.Success)
                {
                    foreach (var item in model.listShipmentSurcharge)
                    {
                        item.Cdclosed = true;
                        item.DatetimeModified = DateTime.Now;
                        item.UserModified = currentUser.UserID; // need update in the future 
                        surchargeRepository.Update(item, x => x.Id == item.Id, false);
                    }
                    UpdateJobModifyTime(model.Id);
                }
                DataContext.SubmitChanges();
                surchargeRepository.SubmitChanges();
                opstransRepository.SubmitChanges();
                cstransRepository.SubmitChanges();

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
                    //List<Guid> lst_Hbid = trandetailRepositoty.Get(x => x.JobId == id);//((eFMSDataContext)DataContext.DC).CsTransactionDetail.Where(x => x.JobId == id).ToList().Select(x => x.Id).ToList();
                    List<CsTransactionDetail> housebills = trandetailRepositoty.Get(x => x.JobId == id).ToList();
                    foreach (var housebill in housebills)
                    {
                        //var houseBill = ((eFMSDataContext)DataContext.DC).CsTransactionDetail.Where(x => x.Id == _id).FirstOrDefault();
                        List<CsShipmentSurchargeDetailsModel> listCharges = new List<CsShipmentSurchargeDetailsModel>();
                        if (housebill != null)
                        {
                            listCharges = Query(housebill.Id);

                            foreach (var c in listCharges)
                            {
                                if (c.PaymentObjectId != null)
                                {
                                    var partner = partnerRepositoty.Get(x => x.Id == c.PaymentObjectId).FirstOrDefault();//((eFMSDataContext)DataContext.DC).CatPartner.Where(x => x.Id == c.PaymentObjectId).FirstOrDefault();
                                    if (partner != null) listPartners.Add(partner);
                                }
                                if (c.PayerId != null)
                                {
                                    var partner = partnerRepositoty.Get(x => x.Id == c.PayerId).FirstOrDefault();//((eFMSDataContext)DataContext.DC).CatPartner.Where(x => x.Id == c.PayerId).FirstOrDefault();
                                    if (partner != null) listPartners.Add(partner);
                                }
                            }
                        }

                    }
                }
                else
                {
                    List<CsShipmentSurchargeDetailsModel> listCharges = new List<CsShipmentSurchargeDetailsModel>();
                    listCharges = Query(id);

                    foreach (var c in listCharges)
                    {
                        if (c.PaymentObjectId != null)
                        {
                            var partner = partnerRepositoty.Get(x => x.Id == c.PaymentObjectId).FirstOrDefault();//((eFMSDataContext)DataContext.DC).CatPartner.Where(x => x.Id == c.PaymentObjectId).FirstOrDefault();
                            if (partner != null) listPartners.Add(partner);
                        }
                        if (c.PayerId != null)
                        {
                            var partner = partnerRepositoty.Get(x => x.Id == c.PayerId).FirstOrDefault();//((eFMSDataContext)DataContext.DC).CatPartner.Where(x => x.Id == c.PayerId).FirstOrDefault();
                            if (partner != null) listPartners.Add(partner);
                        }
                    }
                }
              
                listPartners = listPartners.Distinct().ToList();
                foreach(var item in listPartners)
                {
                    var jobId = IsHouseBillID == true ? opstransRepository.Get(x => x.Hblid == id).FirstOrDefault()?.Id : id;
                    var cdNotes = DataContext.Where(x => x.PartnerId == item.Id && x.JobId== jobId).ToList();
                    List<object> listCDNote = new List<object>();
                    foreach(var cdNote in cdNotes)
                    {
                        // -to continue
                        //var chargesOfCDNote = ((eFMSDataContext)DataContext.DC).CsShipmentSurcharge.Where(x => x.Cdno == cdNote.Code).ToList();
                        var chargesOfCDNote = surchargeRepository.Get(x => x.CreditNo == cdNote.Code || x.DebitNo == cdNote.Code);//((eFMSDataContext)DataContext.DC).CsShipmentSurcharge.Where(x => x.CreditNo == cdNote.Code || x.DebitNo == cdNote.Code).ToList();
                        listCDNote.Add(new { cdNote, total_charge= chargesOfCDNote.Count() });                      

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

        private List<CsShipmentSurchargeDetailsModel> Query(Guid hbId)
        {
            //List<CsShipmentSurchargeDetailsModel> listCharges = new List<CsShipmentSurchargeDetailsModel>();
            //var charges = surchargeRepository.Get(x => x.Hblid == hbId);
            //var partners = partnerRepositoty.Get();
            //var catcharges = catchargeRepository.Get();
            //var currencies = currencyRepository.Get();
            //var units = unitRepository.Get();

            //var query = (from charge in charges
            //             //where charge.Hblid == hbId
            //             join chargeDetail in catcharges on charge.ChargeId equals chargeDetail.Id

            //             join partner in partners on charge.PaymentObjectId equals partner.Id into partnerGroup
            //             from p in partnerGroup.DefaultIfEmpty()

            //             join payer in partners on charge.PayerId equals payer.Id into payerGroup
            //             from pay in payerGroup.DefaultIfEmpty()

            //             join unit in units on charge.UnitId equals unit.Id
            //             join currency in currencies on charge.CurrencyId equals currency.Id
            //             select new { charge, p, pay, unit.UnitNameEn, currency.CurrencyName, chargeDetail.ChargeNameEn, chargeDetail.Code }
            //            ).ToList();
            //foreach (var item in query)
            //{
            //    var charge = mapper.Map<CsShipmentSurchargeDetailsModel>(item.charge);
            //    charge.PartnerName = item.p?.PartnerNameEn;
            //    charge.NameEn = item?.ChargeNameEn;
            //    if(charge.Type == "OBH")
            //    {
            //        charge.ReceiverName = item.p?.PartnerNameEn;
            //    }
            //    charge.PayerName = item.pay?.PartnerNameEn;
            //    charge.Unit = item.UnitNameEn;
            //    charge.Currency = item.CurrencyName;
            //    charge.ChargeCode = item.Code;
            //    listCharges.Add(charge);
            //}
            var surcharges = surchargeService.GetByHB(hbId);
            return surcharges;
        }

        public AcctCDNoteDetailsModel GetCDNoteDetails(Guid JobId, string cdNo)
        {
            var places = placeRepository.Get();
            AcctCDNoteDetailsModel soaDetails = new AcctCDNoteDetailsModel();
            var cdNote = DataContext.Where(x => x.Code == cdNo).FirstOrDefault();
            var partner = partnerRepositoty.Get(x => x.Id == cdNote.PartnerId).FirstOrDefault();

            CatPlace pol = new CatPlace();
            CatPlace pod = new CatPlace();

            var transaction = cstransRepository.Get(x => x.Id == JobId).FirstOrDefault();
            var opsTransaction = opstransRepository.Get(x => x.Id == JobId).FirstOrDefault();
            string warehouseId = null;
            if (transaction != null)
            {
                pol = places.FirstOrDefault(x => x.Id == transaction.Pol);
                pod = places.FirstOrDefault(x => x.Id == transaction.Pod);
                warehouseId = transaction.WareHouseId ?? null;
            }
            else
            {
                pol = places.FirstOrDefault(x => x.Id == opsTransaction.Pol);
                pod = places.FirstOrDefault(x => x.Id == opsTransaction.Pod);
                warehouseId = opsTransaction.WarehouseId?.ToString();
            }
            if ((transaction == null && opsTransaction == null) || cdNote == null || partner==null)
            {
                return null;
            }
            //to continue
            //var charges = ((eFMSDataContext)DataContext.DC).CsShipmentSurcharge.Where(x => x.Cdno == CDNoteCode).ToList();
            var charges = surchargeRepository.Get(x => x.CreditNo == cdNo || x.DebitNo == cdNo).ToList();

            List<CsTransactionDetail> HBList = new List<CsTransactionDetail>();
            List<CsShipmentSurchargeDetailsModel> listSurcharges = new List<CsShipmentSurchargeDetailsModel>();
            if(warehouseId != null)
            {
                soaDetails.WarehouseName = places.FirstOrDefault(x => x.Id == new Guid(warehouseId))?.NameEn;
            }
            soaDetails.CreatedDate = ((DateTime)cdNote.DatetimeCreated).ToString("dd'/'MM'/'yyyy");
            foreach (var item in charges)
            {
                var charge = mapper.Map<CsShipmentSurchargeDetailsModel>(item);               
                var hb = trandetailRepositoty.Get(x => x.Id == item.Hblid).FirstOrDefault();
                var catCharge = ((eFMSDataContext)DataContext.DC).CatCharge.First(x => x.Id == charge.ChargeId);
                var exchangeRate = ((eFMSDataContext)DataContext.DC).CatCurrencyExchange.Where(x => (x.DatetimeCreated.Value.Date == item.ExchangeDate.Value.Date && x.CurrencyFromId == item.CurrencyId && x.CurrencyToId == "VND" && x.Inactive == false)).OrderByDescending(x => x.DatetimeModified).FirstOrDefault();

                charge.Currency = ((eFMSDataContext)DataContext.DC).CatCurrency.First(x => x.Id == charge.CurrencyId).CurrencyName;
                charge.ExchangeRate = (exchangeRate != null && exchangeRate.Rate != 0) ? exchangeRate.Rate : 1;
                charge.Hwbno = hb!=null ? hb.Hwbno : opsTransaction.Hwbno;
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
                soaDetails.CBM = opsTransaction.SumCbm;
                soaDetails.GW = opsTransaction.SumGrossWeight;
                soaDetails.ServiceDate = opsTransaction.ServiceDate;
                soaDetails.HbLadingNo = opsTransaction?.Hwbno;
                soaDetails.MbLadingNo = opsTransaction?.Mblno;
                soaDetails.SumContainers = opsTransaction.SumContainers;
                soaDetails.SumPackages = opsTransaction.SumPackages;
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
            soaDetails.JobId = transaction != null ? transaction.Id : opsTransaction.Id;
            soaDetails.JobNo = transaction != null ? transaction.JobNo : opsTransaction.JobNo;
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
            soaDetails.Vessel = transaction != null ? transaction.FlightVesselName : opsTransaction.FlightVessel;
            soaDetails.HbConstainers = hbConstainers;
            soaDetails.Etd = transaction != null ? transaction.Etd : opsTransaction.ServiceDate;
            soaDetails.Eta = transaction != null ? transaction.Eta : opsTransaction.FinishDate;
            soaDetails.IsLocked = false;
            soaDetails.Volum = volum;
            soaDetails.ListSurcharges = listSurcharges;
            soaDetails.CDNote = cdNote;
            soaDetails.ProductService = opsTransaction.ProductService;
            soaDetails.ServiceMode = opsTransaction.ServiceMode;

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
                    hs = new HandleState(stringLocalizer[LanguageSub.MSG_CDNOTE_NOT_ALLOW_DELETED_NOT_FOUND].Value);
                }
                else
                {
                    //to continue
                    //var charges = ((eFMSDataContext)DataContext.DC).CsShipmentSurcharge.Where(x => x.Cdno == cdNote.Code).ToList();
                    var charges = surchargeRepository.Get(x => x.CreditNo == cdNote.Code || x.DebitNo == cdNote.Code); //((eFMSDataContext)DataContext.DC).CsShipmentSurcharge.Where(x => x.CreditNo == cdNote.Code || x.DebitNo == cdNote.Code).ToList();
                    var isOtherSOA = false;
                    foreach(var item in charges)
                    {
                        isOtherSOA |= (item.Soano != null || item.PaySoano != null);
                    }
                    if (isOtherSOA == true)
                    {
                        hs = new HandleState(stringLocalizer[LanguageSub.MSG_CDNOTE_NOT_ALLOW_DELETED_HAD_SOA].Value);
                    }
                    else
                    {                       
                        var _hs = DataContext.Delete(x => x.Id == idSoA, false);
                        if (hs.Success)
                        {
                            foreach (var item in charges)
                            {
                                //-to contiue
                                //item.Cdno = null;
                                //if(item.Type == "BUY")
                                //{
                                //    item.CreditNo = null;
                                //}
                                //else if(item.Type == "BUY")
                                //{
                                //    item.DebitNo = null;
                                //}
                                //else
                                //{
                                //    if(item.DebitNo == cdNote.Code)
                                //    {
                                //        item.DebitNo = null;
                                //    }
                                //    if(item.CreditNo == cdNote.Code)
                                //    {
                                //        item.CreditNo = null;
                                //    }
                                //}
                                if (item.Type == "BUY")
                                {
                                    item.CreditNo = null;
                                }
                                else if (item.Type == "SELL")
                                {
                                    item.DebitNo = null;
                                }
                                else
                                {
                                    if (item.DebitNo == cdNote.Code)
                                    {
                                        item.DebitNo = null;
                                    }
                                    if (item.CreditNo == cdNote.Code)
                                    {
                                        item.CreditNo = null;
                                    }
                                }
                                item.UserModified = cdNote.UserModified;
                                item.DatetimeModified = DateTime.Now;
                                surchargeRepository.Update(item, x => x.Id == item.Id, false);
                            }
                            ((eFMSDataContext)DataContext.DC).SaveChanges();
                            var jobOpsTrans = opstransRepository.Get(x => x.Id == cdNote.JobId).FirstOrDefault();//((eFMSDataContext)DataContext.DC).OpsTransaction.FirstOrDefault();
                            if (jobOpsTrans != null)
                            {
                                jobOpsTrans.UserModified = currentUser.UserID;
                                jobOpsTrans.ModifiedDate = DateTime.Now;
                                opstransRepository.Update(jobOpsTrans, x => x.Id == jobOpsTrans.Id, false);//((eFMSDataContext)DataContext.DC).OpsTransaction.Update(jobOpsTrans);
                            }
                            var jobCSTrans = cstransRepository.Get(x => x.Id == cdNote.JobId).FirstOrDefault(); //((eFMSDataContext)DataContext.DC).CsTransaction.FirstOrDefault();
                            if (jobCSTrans != null)
                            {
                                jobCSTrans.UserModified = currentUser.UserID;
                                jobCSTrans.ModifiedDate = DateTime.Now;
                                cstransRepository.Update(jobCSTrans, x => x.Id == jobCSTrans.Id, false);//((eFMSDataContext)DataContext.DC).CsTransaction.Update(jobCSTrans);
                            }
                        }
                        DataContext.SubmitChanges();
                        surchargeRepository.SubmitChanges();
                        opstransRepository.SubmitChanges();
                        cstransRepository.SubmitChanges();
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

        private void UpdateJobModifyTime(Guid jobId)
        {
            var jobOpsTrans = opstransRepository.First(x => x.Id == jobId);
            if (jobOpsTrans != null)
            {
                jobOpsTrans.UserModified = currentUser.UserID;
                jobOpsTrans.ModifiedDate = DateTime.Now;
                opstransRepository.Update(jobOpsTrans, x => x.Id == jobId, false);
            }
            var jobCSTrans = cstransRepository.First(x => x.Id == jobId);
            if (jobCSTrans != null)
            {
                jobCSTrans.UserModified = currentUser.UserID;
                jobCSTrans.ModifiedDate = DateTime.Now;
                cstransRepository.Update(jobCSTrans, x => x.Id == jobId, false);
            }
        }
    }
}
