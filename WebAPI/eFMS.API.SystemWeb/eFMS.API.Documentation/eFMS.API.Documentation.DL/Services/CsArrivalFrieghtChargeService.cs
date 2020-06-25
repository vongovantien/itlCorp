using AutoMapper;
using AutoMapper.QueryableExtensions;
using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.DL.Models.ReportResults;
using eFMS.API.Documentation.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Documentation.DL.Services
{
    public class CsArrivalFrieghtChargeService : RepositoryBase<CsArrivalFrieghtCharge, CsArrivalFrieghtChargeModel>, ICsArrivalFrieghtChargeService
    {
        private readonly IContextBase<CsTransactionDetail> detailTransactionRepository;
        private readonly IContextBase<CsArrivalFrieghtChargeDefault> freightChargeDefaultRepository;
        private readonly IContextBase<CsArrivalAndDeliveryDefault> arrivalDeliveryDefaultRepository;
        private readonly IContextBase<CsTransaction> transactionRepository;
        private readonly ICurrentUser currentUser;
        private readonly IContextBase<CatCharge> chargeRepository;
        private readonly IContextBase<CatUnit> unitRepository;
        private readonly IStringLocalizer stringLocalizer;
        private readonly IContextBase<CatPlace> placeRepository;
        private readonly IContextBase<CsMawbcontainer> containerRepository;
        readonly IContextBase<CatPartner> partnerRepositoty;
        ICsTransactionDetailService houseBills;

        public CsArrivalFrieghtChargeService(IStringLocalizer<LanguageSub> localizer,
            IContextBase<CsArrivalFrieghtCharge> repository,
            IMapper mapper,
            IContextBase<CsTransactionDetail> detailTransaction,
            IContextBase<CsArrivalFrieghtChargeDefault> freightChargeDefault,
            IContextBase<CsArrivalAndDeliveryDefault> arrivalDeliveryDefault,
            ICurrentUser currUser,
            IContextBase<CsTransaction> transactionRepo,
            IContextBase<CatCharge> chargeRepo,
            IContextBase<CatUnit> unitRepo,
            IContextBase<CatPlace> placeRepo,
            IContextBase<CsMawbcontainer> containerRepo,
            IContextBase<CatPartner> partnerRepo,
            ICsTransactionDetailService houseBill
            ) : base(repository, mapper)
        {
            stringLocalizer = localizer;
            detailTransactionRepository = detailTransaction;
            freightChargeDefaultRepository = freightChargeDefault;
            arrivalDeliveryDefaultRepository = arrivalDeliveryDefault;
            currentUser = currUser;
            transactionRepository = transactionRepo;
            chargeRepository = chargeRepo;
            unitRepository = unitRepo;
            placeRepository = placeRepo;
            containerRepository = containerRepo;
            partnerRepositoty = partnerRepo;
            houseBills = houseBill;
        }

        #region Arrival
        public CsArrivalViewModel GetArrival(Guid hblid, string transactionType)
        {
            CsArrivalViewModel result = new CsArrivalViewModel { HBLID = hblid };
            var data = detailTransactionRepository.Get(x => x.Id == hblid)?.FirstOrDefault();
            if (data != null)
            {
                result.ArrivalNo = data.ArrivalNo;
                result.ArrivalHeader = data.ArrivalHeader;
                result.ArrivalFirstNotice = data.ArrivalFirstNotice;
                result.ArrivalSecondNotice = data.ArrivalSecondNotice;
                result.ArrivalFooter = data.ArrivalFooter;

                result.CsArrivalFrieghtCharges = GetFreightCharge(hblid);
            }
            else
            {
                var arrivalDefault = GetArrivalDefault(transactionType, currentUser.UserID);
                if (arrivalDefault != null)
                {
                    result.ArrivalHeader = arrivalDefault.ArrivalHeader;
                    result.ArrivalFooter = arrivalDefault.ArrivalFooter;
                }
                result.CsArrivalFrieghtCharges = GetFreightChargeDefault(hblid, transactionType);
            }
            return result;
        }

        private List<CsArrivalFrieghtChargeModel> GetFreightCharge(Guid hblid)
        {
            var result = new List<CsArrivalFrieghtChargeModel>();
            var freightCharges = DataContext.Get(x => x.Hblid == hblid).ProjectTo<CsArrivalFrieghtChargeModel>(mapper.ConfigurationProvider);
            if (freightCharges.Count() > 0)
            {
                var charges = chargeRepository.Get();
                var units = unitRepository.Get();
                var tempdata = freightCharges.Join(charges, x => x.ChargeId, y => y.Id, (x, y) => new { ArrivalFrieghtCharge = x, ChargeCode = y.Code, ChargeName = y.ChargeNameEn })
                    .Join(units, x => x.ArrivalFrieghtCharge.UnitId, y => y.Id, (x, y) => new { CsArrivalFrieghtCharge = x, UnitName = y.Code });
                foreach (var item in tempdata)
                {
                    var charge = item.CsArrivalFrieghtCharge.ArrivalFrieghtCharge;
                    charge.CurrencyName = charge.CurrencyId;
                    charge.ChargeName = item.CsArrivalFrieghtCharge.ChargeName;
                    charge.UnitName = item.UnitName;
                    charge.ChargeCode = item.CsArrivalFrieghtCharge.ChargeCode;
                    result.Add(charge);
                }
            }
            return result;
        }

        private List<CsArrivalFrieghtChargeModel> GetFreightChargeDefault(Guid hblid, string transactionType)
        {
            var results = new List<CsArrivalFrieghtChargeModel>();
            var defaultFreightCharges = freightChargeDefaultRepository.Get(x => x.UserDefault == currentUser.UserID && x.TransactionType == transactionType)
                .Select(x => new CsArrivalFrieghtChargeModel
                {
                    Id = Guid.Empty,
                    Hblid = hblid,
                    Description = null,
                    ChargeId = x.ChargeId,
                    Quantity = x.Quantity,
                    UnitId = x.UnitId,
                    UnitPrice = x.UnitPrice,
                    CurrencyId = x.CurrencyId,
                    Vatrate = x.Vatrate,
                    Total = x.Total,
                    ExchangeRate = x.ExchangeRate,
                    Notes = x.Notes,
                    IsShow = x.IsShow,
                    IsFull = x.IsFull,
                    IsTick = x.IsTick
                });
            if (defaultFreightCharges.Count() > 0)
            {
                var charges = chargeRepository.Get();
                var units = unitRepository.Get();
                var tempdata = defaultFreightCharges.Join(charges, x => x.ChargeId, y => y.Id, (x, y) => new { FreightCharge = x, ChargeName = y.Code })
                    .Join(units, x => x.FreightCharge.UnitId, y => y.Id, (x, y) => new { x, y.Code });
                results = new List<CsArrivalFrieghtChargeModel>();
                foreach (var item in tempdata)
                {
                    var charge = item.x.FreightCharge;
                    charge.ChargeName = item.x.ChargeName;
                    charge.UnitName = item.Code;
                    results.Add(charge);
                }
            }
            return results;
        }

        public CsArrivalDefaultModel GetArrivalDefault(string transactionType, string userDefault)
        {
            var data = arrivalDeliveryDefaultRepository.Get(x => x.TransactionType == transactionType && x.UserDefault == userDefault)?.FirstOrDefault();
            if (data != null)
            {
                var result = new CsArrivalDefaultModel
                {
                    UserDefault = userDefault,
                    TransactionType = transactionType,
                    ArrivalHeader = data.ArrivalHeader,
                    ArrivalFooter = data.ArrivalFooter
                };
                return result;
            }
            return null;
        }

        public HandleState UpdateArrival(CsArrivalViewModel model)
        {
            var detailTransaction = detailTransactionRepository.First(x => x.Id == model.HBLID);
            if (detailTransaction == null) return new HandleState(stringLocalizer[LanguageSub.MSG_DATA_NOT_FOUND].Value);
            detailTransaction.ArrivalNo = model.ArrivalNo;
            detailTransaction.ArrivalFirstNotice = model.ArrivalFirstNotice;
            detailTransaction.ArrivalSecondNotice = model.ArrivalSecondNotice;
            detailTransaction.ArrivalHeader = model.ArrivalHeader;
            detailTransaction.ArrivalFooter = model.ArrivalFooter;
            var hs = detailTransactionRepository.Update(detailTransaction, x => x.Id == model.HBLID, false);
            if (hs.Success)
            {
                var oldCharges = DataContext.Get(x => x.Hblid == model.HBLID);
                foreach (var item in oldCharges)
                {
                    DataContext.Delete(x => x.Id == item.Id, false);
                }
                foreach (var item in model.CsArrivalFrieghtCharges)
                {
                    item.Id = Guid.NewGuid();
                    item.Hblid = model.HBLID;
                    item.UserCreated = item.UserModified = currentUser.UserID;
                    item.DatetimeCreated = item.DatetimeModified = DateTime.Now;
                    DataContext.Add(item, false);
                }
                detailTransactionRepository.SubmitChanges();
                DataContext.SubmitChanges();
            }
            return hs;
        }

        public HandleState SetArrivalChargeDefault(CsArrivalFrieghtChargeDefaultEditModel model)
        {
            model.TransactionType = DataTypeEx.GetType(model.Type);
            var charges = freightChargeDefaultRepository.Get(x => x.UserDefault == model.UserDefault && x.TransactionType == model.TransactionType);
            if (charges != null)
            {
                foreach (var item in charges)
                {
                    freightChargeDefaultRepository.Delete(x => x.UserDefault == model.UserDefault && x.TransactionType == model.TransactionType, false);
                }
            }
            foreach (var item in model.CsArrivalFrieghtChargeDefaults)
            {
                item.UserDefault = model.UserDefault;
                item.TransactionType = model.TransactionType;
                item.UserCreated = item.UserModified = currentUser.UserID;
                item.DatetimeCreated = item.DatetimeModified = DateTime.Now;
                freightChargeDefaultRepository.Add(item, false);
            }
            freightChargeDefaultRepository.SubmitChanges();
            return new HandleState();
        }

        public HandleState SetArrivalHeaderFooterDefault(CsArrivalDefaultModel model)
        {
            model.TransactionType = DataTypeEx.GetType(model.Type);
            var result = new HandleState();
            var data = arrivalDeliveryDefaultRepository.Get(x => x.UserDefault == model.UserDefault && x.TransactionType == model.TransactionType)?.FirstOrDefault();
            if (data != null)
            {
                var arrivalHeaderDefault = data;
                arrivalHeaderDefault.ArrivalHeader = model.ArrivalHeader;
                arrivalHeaderDefault.ArrivalFooter = model.ArrivalFooter;
                result = arrivalDeliveryDefaultRepository.Update(arrivalHeaderDefault, x => x.UserDefault == model.UserDefault && x.TransactionType == model.TransactionType);
            }
            else
            {
                var arrivalHeaderDefault = new CsArrivalAndDeliveryDefault
                {
                    ArrivalHeader = model.ArrivalHeader,
                    ArrivalFooter = model.ArrivalFooter,
                    UserDefault = model.UserDefault,
                    TransactionType = model.TransactionType
                };
                result = arrivalDeliveryDefaultRepository.Add(arrivalHeaderDefault);
            }
            return result;
        }

        public Crystal PreviewArrivalNoticeSIF(PreviewArrivalNoticeCriteria criteria)
        {
            Crystal result = null;
            var _currentUser = currentUser.UserName;
            var listCharge = new List<SeaArrivalNotesReport>();

            var arrival = GetArrival(criteria.HblId, string.Empty);
            var houserBill = houseBills.GetById(criteria.HblId);
            if (arrival != null && houserBill != null)
            {
                var polName = placeRepository.Get(x => x.Id == houserBill.Pol).FirstOrDefault()?.NameEn;
                var podName = placeRepository.Get(x => x.Id == houserBill.Pod).FirstOrDefault()?.NameEn;

                var _arrivalHeader = ReportUltity.ReplaceHtmlBaseForPreviewReport(arrival.ArrivalHeader);
                var _arrivalFooter = ReportUltity.ReplaceHtmlBaseForPreviewReport(arrival.ArrivalFooter);
                var packageUnit = unitRepository.Get(x => x.Id == houserBill.PackageType).FirstOrDefault()?.Code;
                var packageQty = houserBill.PackageQty?.ToString() ?? string.Empty;

                if (arrival.CsArrivalFrieghtCharges.Count > 0)
                {
                    foreach (var frieght in arrival.CsArrivalFrieghtCharges)
                    {
                        var charge = new SeaArrivalNotesReport();
                        charge.HWBNO = houserBill.Hwbno?.ToUpper();
                        charge.ArrivalNo = arrival.ArrivalNo?.ToUpper();
                        charge.ReferrenceNo = houserBill.ReferenceNo?.ToUpper() ?? string.Empty;
                        charge.ISSUED = DateTime.Now.ToString("dd/MM/yyyy");//Issued Date
                        charge.ATTN = houserBill.ShipperDescription?.ToUpper();//Shipper
                        charge.Consignee = houserBill.ConsigneeDescription?.ToUpper();
                        charge.Notify = houserBill.NotifyPartyDescription?.ToUpper();
                        charge.HandlingInfo = string.Empty;
                        charge.ExecutedOn = string.Empty;
                        charge.OceanVessel = houserBill.OceanVessel?.ToUpper();
                        charge.OSI = _arrivalHeader;//Header of arrival (Không UpperCase)
                        if (houserBill.Eta != null)
                        {
                            charge.FlightDate = houserBill.Eta.Value; //ETA
                        }
                        charge.DateConfirm = DateTime.Now;
                        charge.DatePackage = DateTime.Now;
                        charge.LocalVessel = houserBill.OceanVessel?.ToUpper();//Ocean Vessel of HBL
                        charge.ContSealNo = houserBill.OceanVoyNo?.ToUpper();// Ocean Voy No of HBL
                        charge.ForCarrier = string.Empty;
                        charge.DepartureAirport = polName?.ToUpper();//POL of HBL
                        charge.PortofDischarge = podName?.ToUpper();//POD of HBL
                        charge.PlaceDelivery = houserBill.FinalDestinationPlace?.ToUpper();//Final Destination
                        charge.ArrivalNote = houserBill.ServiceType?.ToUpper(); //Lấy Service Type [houserBill.Remark?.ToUpper();//Remark of HBL]

                        charge.ShippingMarkImport = houserBill.ContSealNo;//Lấy value của field Container No/Container Type/Seal No

                        charge.TotalPackages = houserBill.PackageContainer;// Detail container & package                    
                        charge.Description = houserBill.DesOfGoods;// Description of goods (Description)             
                        charge.NoPieces = (packageQty != "0" && packageQty != string.Empty) ? packageQty + " " + packageUnit : packageQty;// Package Qty + Package Unit
                        charge.GrossWeight = houserBill.GrossWeight ?? 0; //GrossWeight of container
                        charge.CBM = houserBill.Cbm ?? 0; //CBM of container
                        charge.Unit = "KGS"; //Đang gán cứng

                        charge.blnShow = frieght.IsShow ?? false; //isShow of charge arrival
                        charge.blnStick = frieght.IsTick ?? false;//isStick of charge arrival
                        charge.blnRoot = frieght.IsFull ?? false; //isRoot of charge arrival
                        charge.FreightCharges = chargeRepository.Get(x => x.Id == frieght.ChargeId).FirstOrDefault()?.ChargeNameEn;//Charge name of charge arrival
                        charge.Qty = frieght.Quantity ?? 0;//Quantity of charge arrival
                        charge.QUnit = frieght.UnitName;//Unit name of charge arrival
                        charge.TotalValue = frieght.UnitPrice ?? 0;//Unit price of charge arrival
                        charge.Curr = frieght.CurrencyId; //Currency of charge arrival
                        charge.VAT = frieght.Vatrate ?? 0; //VAT of charge arrival
                        charge.Notes = frieght.Notes;//Note of charge arrival
                        charge.ArrivalFooterNoitice = _arrivalFooter;//Footer of arrival (Không UpperCase)
                        charge.SeaFCL = true; //Đang gán cứng lấy hàng nguyên công
                        charge.MaskNos = "MaskNos";
                        charge.DlvCustoms = "DlvCustoms";
                        charge.insurAmount = "insurAmount";
                        charge.BillType = houserBill.Hbltype; // House Bill of Lading Type
                        charge.DOPickup = DateTime.Now;
                        charge.ExVND = frieght.ExchangeRate ?? 0;
                        charge.DecimalSymbol = ".";//Dấu phân cách phần thập phân
                        charge.DigitSymbol = ",";//Dấu phân cách phần ngàn
                        charge.DecimalNo = 3;
                        charge.CurrDecimalNo = 2;

                        listCharge.Add(charge);
                    }
                }
                else
                {
                    var charge = new SeaArrivalNotesReport();
                    charge.HWBNO = houserBill.Hwbno?.ToUpper();
                    charge.ArrivalNo = arrival.ArrivalNo?.ToUpper();
                    charge.ReferrenceNo = houserBill.ReferenceNo?.ToUpper() ?? string.Empty;
                    charge.ISSUED = DateTime.Now.ToString("dd/MM/yyyy");//Issued Date
                    charge.ATTN = houserBill.ShipperDescription?.ToUpper();//Shipper
                    charge.Consignee = houserBill.ConsigneeDescription?.ToUpper();
                    charge.Notify = houserBill.NotifyPartyDescription?.ToUpper();
                    charge.HandlingInfo = string.Empty;
                    charge.ExecutedOn = string.Empty;
                    charge.OceanVessel = houserBill.OceanVessel?.ToUpper();
                    charge.OSI = _arrivalHeader;//Header of arrival (Không UpperCase)
                    if (houserBill.Eta != null)
                    {
                        charge.FlightDate = houserBill.Eta.Value; //ETA
                    }
                    charge.DateConfirm = DateTime.Now;
                    charge.DatePackage = DateTime.Now;
                    charge.LocalVessel = houserBill.OceanVessel?.ToUpper();//Ocean Vessel of HBL
                    charge.ContSealNo = houserBill.OceanVoyNo?.ToUpper();// Ocean Voy No of HBL
                    charge.ForCarrier = string.Empty;
                    charge.DepartureAirport = polName?.ToUpper();//POL of HBL
                    charge.PortofDischarge = podName?.ToUpper();//POD of HBL
                    charge.PlaceDelivery = houserBill.FinalDestinationPlace?.ToUpper();//Final Destination
                    charge.ArrivalNote = houserBill.ServiceType?.ToUpper(); //Lấy Service Type [houserBill.Remark?.ToUpper();//Remark of HBL]

                    charge.ShippingMarkImport = houserBill.ContSealNo;//Lấy value của field Container No/Container Type/Seal No

                    charge.TotalPackages = houserBill.PackageContainer;// Detail container & package                    
                    charge.Description = houserBill.DesOfGoods;// Description of goods (Description)
                    charge.NoPieces = (packageQty != "0" && packageQty != string.Empty) ? packageQty + " " + packageUnit : packageQty;// Package Qty + Package Unit
                    charge.GrossWeight = houserBill.GrossWeight ?? 0; //GrossWeight of container
                    charge.CBM = houserBill.Cbm ?? 0; //CBM of container
                    charge.Unit = "KGS"; //Đang gán cứng

                    charge.blnShow = false; //isShow of charge arrival
                    charge.blnStick = false;//isStick of charge arrival
                    charge.blnRoot = false; //isRoot of charge arrival
                    charge.FreightCharges = string.Empty;//Charge name of charge arrival
                    charge.Qty = 0;//Quantity of charge arrival
                    charge.QUnit = string.Empty;//Unit name of charge arrival
                    charge.TotalValue = 0;//Unit price of charge arrival
                    charge.Curr = string.Empty; //Currency of charge arrival
                    charge.VAT = 0; //VAT of charge arrival
                    charge.Notes = string.Empty;//Note of charge arrival
                    charge.ArrivalFooterNoitice = _arrivalFooter;//Footer of arrival (Không UpperCase)
                    charge.SeaFCL = true; //Đang gán cứng lấy hàng nguyên công
                    charge.MaskNos = "MaskNos";
                    charge.DlvCustoms = "DlvCustoms";
                    charge.insurAmount = "insurAmount";
                    charge.BillType = houserBill.Hbltype; // House Bill of Lading Type
                    charge.DOPickup = DateTime.Now;
                    charge.ExVND = 0;
                    charge.DecimalSymbol = ".";//Dấu phân cách phần thập phân
                    charge.DigitSymbol = ",";//Dấu phân cách phần ngàn
                    charge.DecimalNo = 3;
                    charge.CurrDecimalNo = 2;

                    listCharge.Add(charge);
                }
            }

            var parameter = new SeaArrivalNotesReportParams();
            parameter.No = string.Empty;
            parameter.ShipperName = string.Empty;
            parameter.CompanyName = DocumentConstants.COMPANY_NAME;
            parameter.CompanyDescription = string.Empty;
            parameter.CompanyAddress1 = DocumentConstants.COMPANY_ADDRESS1;
            parameter.CompanyAddress2 = DocumentConstants.COMPANY_CONTACT;
            parameter.Website = DocumentConstants.COMPANY_WEBSITE;
            parameter.MAWB = houserBill != null ? houserBill.Mawb?.ToUpper() : string.Empty;
            parameter.Contact = _currentUser;
            parameter.DecimalNo = 3;
            parameter.CurrDecimalNo = 0;
            parameter.Day = arrival != null ? arrival.ArrivalFirstNotice?.ToString("dd") : string.Empty;
            parameter.Month = arrival != null ? arrival.ArrivalFirstNotice?.ToString("MM") : string.Empty;
            parameter.Year = arrival != null ? arrival.ArrivalFirstNotice?.ToString("yyyy") : string.Empty;
            result = new Crystal
            {
                ReportName = criteria.Currency == DocumentConstants.CURRENCY_LOCAL ? "SeaArrivalNotes.rpt" : "SeaArrivalNotesOG.rpt",
                AllowPrint = true,
                AllowExport = true
            };
            result.AddDataSource(listCharge);
            result.FormatType = ExportFormatType.PortableDocFormat;
            result.SetParameter(parameter);
            return result;
        }

        public Crystal PreviewArrivalNoticeAir(PreviewArrivalNoticeCriteria criteria)
        {
            Crystal result = null;
            var _currentUser = currentUser.UserName;
            var listCharge = new List<AirImptArrivalReport>();
            var arrival = GetArrival(criteria.HblId, string.Empty);
            var houseBill = houseBills.GetById(criteria.HblId);
            if (arrival != null && houseBill != null)
            {
                var _polName = placeRepository.Get(x => x.Id == houseBill.Pol).FirstOrDefault()?.NameEn;
                var _podName = placeRepository.Get(x => x.Id == houseBill.Pod).FirstOrDefault()?.NameEn;
                var _shipperName = partnerRepositoty.Get(x => x.Id == houseBill.ShipperId).FirstOrDefault()?.PartnerNameEn;
                //var _consigneeName = partnerRepositoty.Get(x => x.Id == houseBill.ConsigneeId).FirstOrDefault()?.PartnerNameEn;
                var _agentName = partnerRepositoty.Get(x => x.Id == houseBill.ForwardingAgentId).FirstOrDefault()?.PartnerNameEn;

                var _arrivalHeader = ReportUltity.ReplaceHtmlBaseForPreviewReport(arrival.ArrivalHeader);
                var _arrivalFooter = ReportUltity.ReplaceHtmlBaseForPreviewReport(arrival.ArrivalFooter);

                if (arrival.CsArrivalFrieghtCharges.Count > 0)
                {
                    foreach (var frieght in arrival.CsArrivalFrieghtCharges)
                    {
                        var charge = new AirImptArrivalReport();
                        charge.HWBNO = houseBill.Hwbno?.ToUpper(); //HWBNO
                        charge.ArrivalNo = arrival.ArrivalNo?.ToUpper(); //ArrivalNo
                        charge.Consignee = houseBill.ConsigneeDescription?.ToUpper();//_consigneeName?.ToUpper();  //Consignee
                        charge.ReferrenceNo = houseBill.ReferenceNo?.ToUpper() ?? string.Empty;
                        charge.FlightNo = houseBill.FlightNo?.ToUpper(); //FlightNo (Arrival)
                        charge.DepartureAirport = _polName?.ToUpper(); //DepartureAirport (POL)
                        charge.CussignedDate = houseBill.FlightDate; //FlightDate (Arrival)
                        charge.LastDestination = _podName?.ToUpper(); //Destination Air Port (POD)
                        charge.ShippingMarkImport = _arrivalHeader; //ArrivalHeader (Không UpperCase)
                        charge.DatePackage = DateTime.Now; //Current Date
                        charge.NoPieces = (houseBill.PackageQty != null ? houseBill.PackageQty.ToString() : string.Empty) + " " + unitRepository.Get(x => x.Id == houseBill.PackageType).FirstOrDefault()?.UnitNameEn; //Quantity + Unit Qty
                        charge.Description = houseBill.DesOfGoods; //Description of Goods
                        charge.WChargeable = houseBill.GrossWeight ?? 0; //G.W (GrossWeight)
                        charge.blnShow = frieght.IsShow ?? false; //isShow of charge arrival
                        charge.blnStick = frieght.IsTick ?? false;//isStick of charge arrival
                        charge.blnRoot = frieght.IsFull ?? false; //isRoot of charge arrival
                        charge.FreightCharge = chargeRepository.Get(x => x.Id == frieght.ChargeId).FirstOrDefault()?.ChargeNameEn;//Charge name of charge arrival
                        charge.Qty = frieght.Quantity ?? 0;//Quantity of charge
                        charge.Unit = frieght.UnitName;//Unit name of charge arrival
                        charge.TotalValue = frieght.UnitPrice ?? 0;//Unit price of charge arrival
                        charge.Curr = frieght.CurrencyId; //Currency of charge arrival
                        charge.VAT = frieght.Vatrate ?? 0; //VAT of charge arrival
                        charge.Notes = frieght.Notes;//Note of charge arrival
                        charge.ArrivalFooterNotice = _arrivalFooter; // Arrival Footer (Không UpperCase)
                        charge.Shipper = _shipperName?.ToUpper(); //Shipper Name
                        charge.CBM = houseBill.ChargeWeight ?? 0; //C.W (ChargeWeight)
                        charge.AOL = string.Empty; //NOT USE
                        charge.KilosUnit = string.Empty; //NOT USE
                        charge.DOPickup = DateTime.Now; //NOT USE
                        charge.ExVND = frieght.ExchangeRate ?? 0;
                        charge.AgentName = _agentName?.ToUpper(); //Agent
                        charge.Notify = houseBill.NotifyParty?.ToUpper(); //Notify Party
                        charge.DecimalSymbol = ".";//Dấu phân cách phần thập phân
                        charge.DigitSymbol = ",";//Dấu phân cách phần ngàn
                        charge.DecimalNo = 0; //NOT USE
                        charge.CurrDecimalNo = 0; //NOT USE

                        listCharge.Add(charge);
                    }
                }
                else
                {
                    var charge = new AirImptArrivalReport();
                    charge.HWBNO = houseBill.Hwbno?.ToUpper(); //HWBNO
                    charge.ArrivalNo = arrival.ArrivalNo?.ToUpper(); //ArrivalNo
                    charge.Consignee = houseBill.ConsigneeDescription?.ToUpper();//_consigneeName?.ToUpper(); //Consignee
                    charge.ReferrenceNo = houseBill.ReferenceNo?.ToUpper() ?? string.Empty;
                    charge.FlightNo = houseBill.FlightNo?.ToUpper(); //FlightNo (Arrival)
                    charge.DepartureAirport = _polName?.ToUpper(); //DepartureAirport (POL)
                    charge.CussignedDate = houseBill.FlightDate; //FlightDate (Arrival)
                    charge.LastDestination = _podName?.ToUpper(); //Destination Air Port (POD)
                    charge.ShippingMarkImport = _arrivalHeader; //ArrivalHeader (Không UpperCase)
                    charge.DatePackage = DateTime.Now; //Current Date
                    charge.NoPieces = (houseBill.PackageQty != null ? houseBill.PackageQty.ToString() : string.Empty) + " " + unitRepository.Get(x => x.Id == houseBill.PackageType).FirstOrDefault()?.UnitNameEn; //Quantity + Unit Qty; //Quantity + Unit Qty
                    charge.Description = houseBill.DesOfGoods; //Description of Goods
                    charge.WChargeable = houseBill.GrossWeight ?? 0; //G.W (GrossWeight)
                    charge.blnShow = false; //isShow of charge arrival
                    charge.blnStick = false;//isStick of charge arrival
                    charge.blnRoot = false; //isRoot of charge arrival
                    charge.FreightCharge = string.Empty;
                    charge.Qty = 0; //Quantity of charge
                    charge.Unit = string.Empty; //Unit of charge
                    charge.TotalValue = 0;
                    charge.Curr = string.Empty;
                    charge.VAT = 0; //VAT of charge
                    charge.Notes = string.Empty;//Note of charge
                    charge.ArrivalFooterNotice = _arrivalFooter; // Arrival Footer (Không UpperCase)
                    charge.Shipper = _shipperName?.ToUpper(); //Shipper
                    charge.CBM = houseBill.ChargeWeight ?? 0; //C.W (ChargeWeight)
                    charge.AOL = string.Empty; //NOT USE
                    charge.KilosUnit = string.Empty; //NOT USE
                    charge.DOPickup = DateTime.Now; //NOT USE
                    charge.ExVND = 0;
                    charge.AgentName = _agentName?.ToUpper(); //Agent
                    charge.Notify = houseBill.NotifyParty?.ToUpper();
                    charge.DecimalSymbol = ".";//Dấu phân cách phần thập phân
                    charge.DigitSymbol = ",";//Dấu phân cách phần ngàn
                    charge.DecimalNo = 0; //NOT USE
                    charge.CurrDecimalNo = 0; //NOT USE

                    listCharge.Add(charge);
                }
            }
            

            var parameter = new AirImptArrivalReportParams();
            parameter.No = string.Empty;
            parameter.MAWB = houseBill != null ? houseBill.Mawb?.ToUpper() : string.Empty;
            parameter.CompanyName = DocumentConstants.COMPANY_NAME;
            parameter.CompanyDescription = string.Empty;
            parameter.CompanyAddress1 = DocumentConstants.COMPANY_ADDRESS1;
            parameter.CompanyAddress2 = DocumentConstants.COMPANY_CONTACT;
            parameter.Website = DocumentConstants.COMPANY_WEBSITE;
            parameter.AccountInfo = string.Empty;
            parameter.Contact = _currentUser;
            parameter.DecimalNo = 3;
            parameter.CurrDecimalNo = 0;

            result = new Crystal
            {
                ReportName = criteria.Currency == DocumentConstants.CURRENCY_LOCAL ? "AirImptArrival.rpt" : "AirImptArrivalOG.rpt",
                AllowPrint = true,
                AllowExport = true
            };
            string folderDownloadReport = CrystalEx.GetFolderDownloadReports();
            var _pathReportGenerate = folderDownloadReport + "\\AirImportArrivalNotice" + DateTime.Now.ToString("ddMMyyHHssmm") + ".pdf";
            result.PathReportGenerate = _pathReportGenerate;

            result.AddDataSource(listCharge);
            result.FormatType = ExportFormatType.PortableDocFormat;
            result.SetParameter(parameter);
            return result;
        }
        #endregion

        #region Delivery Order

        public DeliveryOrderViewModel GetDeliveryOrder(Guid hblid, string transactionType)
        {
            DeliveryOrderViewModel result = new DeliveryOrderViewModel
            {
                HBLID = hblid
            };
            var data = detailTransactionRepository.Get(x => x.Id == hblid)?.FirstOrDefault();
            if (data != null)
            {
                if (string.IsNullOrEmpty(data.DosentTo1) && data.Pod != null)
                {
                    var warehouseId = placeRepository.Get(x => x.Id == data.Pod).FirstOrDefault()?.WarehouseId;
                    if(warehouseId != null)
                    {
                        result.Doheader1 = placeRepository.Get(x => x.Id == warehouseId).FirstOrDefault()?.NameVn;
                    }
                }
                else
                {
                    result.Doheader1 = data.DosentTo1;
                }
                result.Doheader2 = data.DosentTo2;
                result.Dofooter = data.Dofooter;
                result.DeliveryOrderNo = data.DeliveryOrderNo;
                result.DeliveryOrderPrintedDate = data.DeliveryOrderPrintedDate;
                result.SubAbbr = data.SubAbbr;
            }
            else
            {
                var deliveryOrder = GetDeliveryOrderDefault(transactionType, currentUser.UserID);
                if (deliveryOrder != null)
                {
                    result.Doheader1 = deliveryOrder.Doheader1;
                    result.Doheader2 = deliveryOrder.Doheader2;
                    result.Dofooter = deliveryOrder.Dofooter;
                }
            }
            return result;
        }

        public CsDeliveryOrderDefaultModel GetDeliveryOrderDefault(string transactionType, string userDefault)
        {
            CsDeliveryOrderDefaultModel result = null;
            var data = arrivalDeliveryDefaultRepository.Get(x => x.TransactionType == transactionType && x.UserDefault == userDefault)?.FirstOrDefault();
            if (data != null)
            {
                result = new CsDeliveryOrderDefaultModel
                {
                    Doheader1 = data.Doheader1,
                    Doheader2 = data.Doheader2,
                    Dofooter = data.Dofooter
                };
            }
            return result;
        }

        public HandleState SetDeliveryOrderHeaderFooterDefault(CsDeliveryOrderDefaultModel model)
        {
            model.TransactionType = DataTypeEx.GetType(model.Type);
            var result = new HandleState();
            var data = arrivalDeliveryDefaultRepository.Get(x => x.UserDefault == model.UserDefault && x.TransactionType == model.TransactionType);
            if (data != null)
            {
                var doDefault = data.FirstOrDefault();
                doDefault.Doheader1 = model.Doheader1;
                doDefault.Doheader2 = model.Doheader2;
                doDefault.Dofooter = model.Dofooter;
                result = arrivalDeliveryDefaultRepository.Update(doDefault, x => x.UserDefault == model.UserDefault && x.TransactionType == model.TransactionType);
            }
            else
            {
                var doDefault = new CsArrivalAndDeliveryDefault
                {
                    Doheader1 = model.Doheader1,
                    Doheader2 = model.Doheader2,
                    Dofooter = model.Dofooter,
                    UserDefault = model.UserDefault,
                    TransactionType = model.TransactionType
                };
                result = arrivalDeliveryDefaultRepository.Add(doDefault);
            }
            return result;
        }

        public HandleState UpdateDeliveryOrder(DeliveryOrderViewModel model)
        {
            var detailTransaction = detailTransactionRepository.First(x => x.Id == model.HBLID);
            if (detailTransaction == null) return new HandleState(stringLocalizer[LanguageSub.MSG_DATA_NOT_FOUND].Value);
            detailTransaction.DosentTo1 = model.Doheader1;
            detailTransaction.DosentTo2 = model.Doheader2;
            detailTransaction.Dofooter = model.Dofooter;
            detailTransaction.DeliveryOrderNo = model.DeliveryOrderNo;
            detailTransaction.DeliveryOrderPrintedDate = model.DeliveryOrderPrintedDate;
            detailTransaction.SubAbbr = model.SubAbbr;
            var result = detailTransactionRepository.Update(detailTransaction, x => x.Id == model.HBLID);
            return result;
        }

        public Crystal PreviewDeliveryOrder(Guid hblid)
        {
            var detail = detailTransactionRepository.First(x => x.Id == hblid);
            if (detail.DeliveryOrderNo == null) return new Crystal();
            var parameter = new SeaDeliveryCommandParam
            {
                Consignee = "s",
                No = "s",
                CompanyName = DocumentConstants.COMPANY_NAME,
                CompanyDescription = "Company Description",
                CompanyAddress1 = DocumentConstants.COMPANY_ADDRESS1,
                CompanyAddress2 = DocumentConstants.COMPANY_CONTACT,
                Website = DocumentConstants.COMPANY_WEBSITE,
                MAWB = detail.Mawb?.ToUpper(),
                Contact = currentUser.UserName,
                DecimalNo = 2
            };
            var polName = placeRepository.Get(x => x.Id == detail.Pol).FirstOrDefault().NameEn;
            var podName = placeRepository.Get(x => x.Id == detail.Pod).FirstOrDefault().NameEn;
            var dataSources = new List<SeaDeliveryCommandResult>();
            var containers = containerRepository.Get(x => x.Hblid == hblid);

            string totalPackages = detail.PackageContainer + "\n" + detail.ContSealNo;
            string nopieces = string.Empty;
            string unitOfMeasures = string.Empty;
            foreach (var cont in containers)
            {
                nopieces = nopieces +  cont.PackageTypeId != null ? (cont.PackageQuantity + " " + unitRepository.Get(x => x.Id == cont.PackageTypeId)?.FirstOrDefault()?.UnitNameEn) : null;
                unitOfMeasures = cont.UnitOfMeasureId != null ? (unitRepository.Get(x => x.Id == cont.UnitOfMeasureId)?.FirstOrDefault()?.UnitNameEn) : null;
            }
            var item = new SeaDeliveryCommandResult
            {
                DONo = detail.DeliveryOrderNo?.ToUpper(),
                LocalVessel = detail.LocalVessel?.ToUpper(),
                Consignee = ReportUltity.ReplaceNullAddressDescription(detail.ConsigneeDescription)?.ToUpper(),
                OceanVessel = detail.OceanVessel?.ToUpper(),
                DepartureAirport = polName?.ToUpper(),
                PortofDischarge = podName?.ToUpper(),
                PlaceDelivery = podName?.ToUpper(),
                HWBNO = detail.Hwbno?.ToUpper(),
                ShippingMarkImport = detail.ShippingMark,
                SpecialNote = "",
                Description = detail.DesOfGoods,
                ContSealNo = detail.LocalVoyNo, //continue
                TotalPackages = totalPackages,
                NoPieces = nopieces,
                GrossWeight = detail.GrossWeight,
                Unit = unitOfMeasures,
                CBM = detail.Cbm ?? 0,
                DeliveryOrderNote = ReportUltity.ReplaceHtmlBaseForPreviewReport(detail.Dofooter), // (Không Upper Case)
                FirstDestination = detail.DosentTo1?.ToUpper(),
                SecondDestination = detail.DosentTo2?.ToUpper(),
                ArrivalNote = detail.ArrivalNo?.ToUpper(),
                FlightDate = detail.Eta,
                BillType = detail.ServiceType?.ToUpper()
            };
            dataSources.Add(item);
            var result = new Crystal
            {
                ReportName = "SeaDeliveryCommand.rpt",
                AllowPrint = true,
                AllowExport = true
            };
            result.AddDataSource(dataSources);
            result.FormatType = ExportFormatType.PortableDocFormat;
            result.SetParameter(parameter);
            return result;
        }
        #endregion
    }
}
