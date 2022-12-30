﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using eFMS.API.Catalogue.DL.Models;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Helpers;
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
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        private readonly IContextBase<CatPartner> partnerRepositoty;
        private readonly IContextBase<SysOffice> officeRepo;
        private readonly IContextBase<SysCompany> companyRepo;
        private readonly IContextBase<CatChargeGroup> chargeGroupRepository;
        private readonly IContextBase<SysUserLevel> userlevelRepository;
        private decimal _decimalNumber = Constants.DecimalNumber;
        private readonly IOptions<WebUrl> webUrl;
        private readonly IOptions<ApiUrl> apiUrl;
        private readonly IContextBase<SysImage> sysImageRepository;
        private readonly ICurrencyExchangeService currencyExchangeService;
        private readonly IStageService stageService;
        private readonly ICsStageAssignedService stageAssignedService;
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
            ICsTransactionDetailService houseBill,
            IContextBase<SysOffice> sysOffice,
            IContextBase<SysCompany> sysCompany,
            IContextBase<SysUserLevel> userlevelRepo,
            IOptions<WebUrl> url,
            IOptions<ApiUrl> aUrl,
            IContextBase<SysImage> sysImageRepo,
            ICurrencyExchangeService currencyExchange,
            IStageService stage,
            ICsStageAssignedService stageAssigned
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
            officeRepo = sysOffice;
            companyRepo = sysCompany;
            userlevelRepository = userlevelRepo;
            apiUrl = aUrl;
            sysImageRepository = sysImageRepo;
            currencyExchangeService = currencyExchange;
            stageService = stage;
            stageAssignedService = stageAssigned;
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
            List<CsArrivalFrieghtChargeModel> result = new List<CsArrivalFrieghtChargeModel>();
            IQueryable<CsArrivalFrieghtChargeModel> freightCharges = DataContext.Get(x => x.Hblid == hblid).ProjectTo<CsArrivalFrieghtChargeModel>(mapper.ConfigurationProvider);

            if (freightCharges.Count() > 0)
            {
                IQueryable<CatCharge> charges = chargeRepository.Get();
                IQueryable<CatUnit> units = unitRepository.Get();

                var tempdata = freightCharges.Join(charges, x => x.ChargeId, y => y.Id, (x, y) => new { ArrivalFrieghtCharge = x, ChargeCode = y.Code, ChargeName = y.ChargeNameEn, ChargeId = y.Id })
                    .Join(units, x => x.ArrivalFrieghtCharge.UnitId, y => y.Id, (x, y) => new { CsArrivalFrieghtCharge = x, UnitName = y.Code });
                foreach (var item in tempdata)
                {
                    CsArrivalFrieghtChargeModel charge = item.CsArrivalFrieghtCharge.ArrivalFrieghtCharge;

                    charge.CurrencyName = charge.CurrencyId;
                    charge.ChargeName = item.CsArrivalFrieghtCharge.ChargeName;
                    charge.UnitName = item.UnitName;
                    charge.ChargeCode = item.CsArrivalFrieghtCharge.ChargeCode;
                    charge.ChargeGroup = charges.Where(x => x.Id == item.CsArrivalFrieghtCharge.ChargeId)?.FirstOrDefault()?.ChargeGroup ?? null;

                    result.Add(charge);
                }
            }
            return result;
        }

        private List<CsArrivalFrieghtChargeModel> GetFreightChargeDefault(Guid hblid, string transactionType)
        {
            List<CsArrivalFrieghtChargeModel> results = new List<CsArrivalFrieghtChargeModel>();

            IQueryable<CsArrivalFrieghtChargeModel> defaultFreightCharges = freightChargeDefaultRepository.Get(x => x.UserDefault == currentUser.UserID && x.TransactionType == transactionType)
                .Select(x => new CsArrivalFrieghtChargeModel
                {
                    Id = Guid.Empty,
                    Hblid = hblid,
                    Description = x.Description,
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
                    IsTick = x.IsTick,
                });
            if (defaultFreightCharges.Count() > 0)
            {
                IQueryable<CatCharge> charges = chargeRepository.Get();
                IQueryable<CatUnit> units = unitRepository.Get();

                var tempdata = defaultFreightCharges.Join(charges, x => x.ChargeId, y => y.Id, (x, y) => new { FreightCharge = x, ChargeName = y.Code, ChargeId = y.Id })
                    .Join(units, x => x.FreightCharge.UnitId, y => y.Id, (x, y) => new { x, y.Code });

                foreach (var item in tempdata)
                {
                    CsArrivalFrieghtChargeModel charge = item.x.FreightCharge;

                    charge.ChargeName = item.x.ChargeName;
                    charge.UnitName = item.Code;
                    charge.ChargeGroup = charges.Where(x => x.Id == item.x.ChargeId)?.FirstOrDefault()?.ChargeGroup ?? null;

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
            IQueryable<CsArrivalFrieghtChargeDefault> charges = freightChargeDefaultRepository.Get(x => x.UserDefault == model.UserDefault && x.TransactionType == model.TransactionType);
            if (charges.Count() > 0)
            {
                foreach (var item in charges)
                {
                    freightChargeDefaultRepository.Delete(x => x.UserDefault == model.UserDefault && x.TransactionType == model.TransactionType, false);
                }
            }
            foreach (var item in model.CsArrivalFrieghtChargeDefaults)
            {
                item.Id = Guid.NewGuid();
                item.UserDefault = model.UserDefault;
                item.TransactionType = model.TransactionType;
                item.UserCreated = item.UserModified = currentUser.UserID;

                // Nếu đang tạo hbl thì tỷ giá theo ngày tạo hbl.
                if (item.CurrencyId != DocumentConstants.CURRENCY_LOCAL && !string.IsNullOrEmpty(model.HblId))
                {
                    CsTransactionDetail hbl = detailTransactionRepository.Get(x => x.Id.ToString() == model.HblId)?.FirstOrDefault();
                    if (hbl != null)
                    {
                        item.ExchangeRate = currencyExchangeService.CurrencyExchangeRateConvert(null, hbl.DatetimeCreated, item.CurrencyId, DocumentConstants.CURRENCY_LOCAL);
                    }
                    else
                    {
                        item.ExchangeRate = currencyExchangeService.CurrencyExchangeRateConvert(null, DateTime.Now, item.CurrencyId, DocumentConstants.CURRENCY_LOCAL);
                    }
                }
                freightChargeDefaultRepository.Add(item, false);
            }
            HandleState hs = freightChargeDefaultRepository.SubmitChanges();
            return hs;
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
                arrivalHeaderDefault.UserModified = currentUser.UserID;
                arrivalHeaderDefault.DatetimeModified = DateTime.Now;

                result = arrivalDeliveryDefaultRepository.Update(arrivalHeaderDefault, x => x.UserDefault == model.UserDefault && x.TransactionType == model.TransactionType);
            }
            else
            {
                var arrivalHeaderDefault = new CsArrivalAndDeliveryDefault
                {
                    ArrivalHeader = model.ArrivalHeader,
                    ArrivalFooter = model.ArrivalFooter,
                    UserDefault = model.UserDefault,
                    TransactionType = model.TransactionType,
                    UserModified = currentUser.UserID,
                    DatetimeModified = DateTime.Now
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
                //var packageUnit = unitRepository.Get(x => x.Id == houserBill.PackageType).FirstOrDefault()?.Code;
                //var packageQty = houserBill.PackageQty?.ToString() ?? string.Empty;

                var containers = containerRepository.Get(x => x.Hblid == houserBill.Id);
                string nopieces = string.Empty;
                //Group by Package Type [CR: 15147 - 18/12/2020]
                var grpPkgsType = containers.GroupBy(g => new { g.PackageTypeId }).Select(s => new { PackageTypeId = s.Key.PackageTypeId, PackageQuantitys = s.Select(se => (decimal?)se.PackageQuantity) });
                foreach (var grp in grpPkgsType)
                {
                    var pkgsType = unitRepository.Get(x => x.Id == grp.PackageTypeId)?.FirstOrDefault()?.Code; //Unit Code
                    var pkgsQty = grp.PackageQuantitys.Sum();
                    nopieces += pkgsQty + " " + pkgsType + "\r\n";
                }

                //List Container không có giá trị thì lấy Pkgs Qty, Pkgs Type của Housebill [CR: 15147 - 18/12/2020]
                if (string.IsNullOrEmpty(nopieces))
                {
                    var hblPkgsType = unitRepository.Get(x => x.Id == houserBill.PackageType).FirstOrDefault()?.Code; //Unit Code
                    var hblPkgsQty = houserBill.PackageQty?.ToString();
                    nopieces = hblPkgsQty + " " + hblPkgsType;
                }

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
                        charge.LocalVessel = houserBill.LocalVessel?.ToUpper();//Arrival Vessel of HBL [CR: 29/12/2020]
                        charge.ContSealNo = houserBill.LocalVoyNo?.ToUpper();// Arrival Voy No of HBL [CR: 29/12/2020]
                        charge.ForCarrier = string.Empty;
                        charge.DepartureAirport = polName?.ToUpper();//POL of HBL
                        charge.PortofDischarge = podName?.ToUpper();//POD of HBL
                        charge.PlaceDelivery = houserBill.FinalDestinationPlace?.ToUpper();//Final Destination
                        charge.ArrivalNote = houserBill.ServiceType?.ToUpper(); //Lấy Service Type [houserBill.Remark?.ToUpper();//Remark of HBL]

                        charge.ShippingMarkImport = houserBill.ContSealNo;//Lấy value của field Container No/Container Type/Seal No

                        charge.TotalPackages = houserBill.PackageContainer;// Detail container & package
                        charge.Description = houserBill.DesOfGoods;// Description of goods (Description)
                        charge.NoPieces = nopieces;
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
                    charge.LocalVessel = houserBill.LocalVessel?.ToUpper();//Arrival Vessel of HBL [CR: 29/12/2020]
                    charge.ContSealNo = houserBill.LocalVoyNo?.ToUpper();// Arrival Voy No of HBL [CR: 29/12/2020]
                    charge.ForCarrier = string.Empty;
                    charge.DepartureAirport = polName?.ToUpper();//POL of HBL
                    charge.PortofDischarge = podName?.ToUpper();//POD of HBL
                    charge.PlaceDelivery = houserBill.FinalDestinationPlace?.ToUpper();//Final Destination
                    charge.ArrivalNote = houserBill.ServiceType?.ToUpper(); //Lấy Service Type [houserBill.Remark?.ToUpper();//Remark of HBL]

                    charge.ShippingMarkImport = houserBill.ContSealNo;//Lấy value của field Container No/Container Type/Seal No

                    charge.TotalPackages = houserBill.PackageContainer;// Detail container & package
                    charge.Description = houserBill.DesOfGoods;// Description of goods (Description)
                    charge.NoPieces = nopieces;
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

            var pic = transactionRepository.Get(x => x.Id == houserBill.JobId).Select(x => x.PersonIncharge).FirstOrDefault();
            var userInfo = userlevelRepository.Get(x => x.UserId == pic).FirstOrDefault();
            // Company Information of Creator
            var companyUser = companyRepo.Get(x => x.Id == userInfo.CompanyId).FirstOrDefault();
            // Office Information of Creator
            var officeUser = officeRepo.Get(x => x.Id == userInfo.OfficeId).FirstOrDefault();
            var parameter = new SeaArrivalNotesReportParams();
            parameter.No = string.Empty;
            parameter.ShipperName = string.Empty;
            parameter.CompanyName = criteria.Currency == DocumentConstants.CURRENCY_LOCAL ? companyUser?.BunameVn : companyUser?.BunameEn; // Company Name En of user
            parameter.CompanyDescription = string.Empty;
            parameter.CompanyAddress1 = criteria.Currency == DocumentConstants.CURRENCY_LOCAL ? officeUser.AddressVn : officeUser.AddressEn; // Office Address En of user
            parameter.CompanyAddress2 = string.Format(@"Tel: {0}    Fax: {1}", officeUser?.Tel ?? string.Empty, officeUser?.Fax ?? string.Empty); // Tel & Fax of Office user
            parameter.Website = companyUser?.Website; // Website Company of user
            parameter.MAWB = houserBill != null ? (houserBill.Mawb?.ToUpper() ?? string.Empty) : string.Empty;
            parameter.Contact = _currentUser;
            parameter.DecimalNo = 3;
            parameter.CurrDecimalNo = 0;
            parameter.Day = arrival != null ? (arrival.ArrivalFirstNotice?.ToString("dd") ?? string.Empty) : string.Empty;
            parameter.Month = arrival != null ? (arrival.ArrivalFirstNotice?.ToString("MM") ?? string.Empty) : string.Empty;
            parameter.Year = arrival != null ? (arrival.ArrivalFirstNotice?.ToString("yyyy") ?? string.Empty) : string.Empty;
            result = new Crystal
            {
                ReportName = criteria.Currency == DocumentConstants.CURRENCY_LOCAL ? "SeaArrivalNotes.rpt" : "SeaArrivalNotesOG.rpt",
                AllowPrint = true,
                AllowExport = true
            };

            // Get path link to report
            CrystalEx._apiUrl = apiUrl.Value.Url;
            string folderDownloadReport = CrystalEx.GetLinkDownloadReports();
            var reportName = arrival.ArrivalNo != null ? arrival.ArrivalNo + ".pdf" : "SeaImportArrivalNotice_" + transactionRepository.Get(x => x.Id == houserBill.JobId).Select(x => x.JobNo).FirstOrDefault() + ".pdf";
            var _pathReportGenerate = folderDownloadReport + "/" + reportName.Replace("/", "_");
            result.PathReportGenerate = _pathReportGenerate;

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
                var _polName = string.Empty;
                var _podName = string.Empty;
                if (string.IsNullOrEmpty(houseBill.PolDescription))
                {
                    _polName = placeRepository.Get(x => x.Id == houseBill.Pol).FirstOrDefault()?.NameEn;
                }
                else
                {
                    _polName = houseBill.PolDescription;
                }
                if (string.IsNullOrEmpty(houseBill.PodDescription))
                {
                    _podName = placeRepository.Get(x => x.Id == houseBill.Pod).FirstOrDefault()?.NameEn;
                }
                else
                {
                    _podName = houseBill.PodDescription;
                }
                var _shipperName = partnerRepositoty.Get(x => x.Id == houseBill.ShipperId).FirstOrDefault()?.PartnerNameEn;
                //var _consigneeName = partnerRepositoty.Get(x => x.Id == houseBill.ConsigneeId).FirstOrDefault()?.PartnerNameEn;
                var _agentName = partnerRepositoty.Get(x => x.Id == houseBill.ForwardingAgentId).FirstOrDefault()?.PartnerNameEn;

                var _arrivalHeader = ReportUltity.ReplaceHtmlBaseForPreviewReport(arrival.ArrivalHeader);
                var _arrivalFooter = ReportUltity.ReplaceHtmlBaseForPreviewReport(arrival.ArrivalFooter);

                string warehouseName = string.Empty;
                if (houseBill.WarehouseId != Guid.Empty)
                {
                    warehouseName = placeRepository.Get(x => x.Id == houseBill.WarehouseId)?.FirstOrDefault()?.DisplayName;
                }

                string _sipperInfo = string.Empty;
                if (!string.IsNullOrEmpty(houseBill.ShipperDescription))
                {
                    _sipperInfo = houseBill.ShipperDescription;
                }
                else
                {
                    if (!string.IsNullOrEmpty(houseBill.ShipperId))
                    {
                        _sipperInfo = partnerRepositoty.Get(x => x.Id == houseBill.ShipperId).FirstOrDefault()?.PartnerNameEn;
                    }
                }

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
                        charge.WarehouseDestination = !string.IsNullOrEmpty(warehouseName) ? string.Format(@"(WH: {0})", warehouseName) : null;
                        charge.ShippingMarkImport = _arrivalHeader; //ArrivalHeader (Không UpperCase)
                        charge.DatePackage = DateTime.Now; //Current Date
                        charge.NoPieces = (houseBill.PackageQty != null ? houseBill.PackageQty.ToString() : string.Empty) + " " + unitRepository.Get(x => x.Id == houseBill.PackageType).FirstOrDefault()?.UnitNameEn; //Quantity + Unit Qty
                        charge.Description = houseBill.DesOfGoods; //Description of Goods
                        charge.WChargeable = houseBill.GrossWeight ?? 0; //G.W (GrossWeight)
                        charge.blnShow = frieght.IsShow ?? false; //isShow of charge arrival
                        charge.blnStick = frieght.IsTick ?? false;//isStick of charge arrival
                        charge.blnRoot = frieght.IsFull ?? false; //isRoot of charge arrival
                        charge.FreightCharge = chargeRepository.Get(x => x.Id == frieght.ChargeId).FirstOrDefault()?.ChargeNameEn;//Charge name of charge arrival
                        charge.Qty = (frieght.Quantity ?? 0) + _decimalNumber;//Quantity of charge
                        charge.Unit = frieght.UnitName;//Unit name of charge arrival
                        charge.TotalValue = (frieght.UnitPrice ?? 0) + _decimalNumber;//Unit price of charge arrival
                        charge.Curr = frieght.CurrencyId; //Currency of charge arrival
                        charge.VAT = (frieght.Vatrate ?? 0) + _decimalNumber; //VAT of charge arrival
                        charge.Notes = frieght.Notes;//Note of charge arrival
                        charge.ArrivalFooterNotice = _arrivalFooter; // Arrival Footer (Không UpperCase)
                        charge.Shipper = _sipperInfo?.ToUpper(); //Shipper Name
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
                    charge.WarehouseDestination = !string.IsNullOrEmpty(warehouseName) ? string.Format(@"(WH: {0})", warehouseName) : null;
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
                    charge.Shipper = _sipperInfo?.ToUpper(); //Shipper
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

            var pic = transactionRepository.Get(x => x.Id == houseBill.JobId).Select(x => x.PersonIncharge).FirstOrDefault();
            var userInfo = userlevelRepository.Get(x => x.UserId == pic).FirstOrDefault();
            // Company Information
            var companyUser = companyRepo.Get(x => x.Id == userInfo.CompanyId).FirstOrDefault();
            // Office Information
            var officeUser = officeRepo.Get(x => x.Id == userInfo.OfficeId).FirstOrDefault();
            var parameter = new AirImptArrivalReportParams();
            parameter.No = string.Empty;
            parameter.MAWB = houseBill != null ? (houseBill.Mawb?.ToUpper() ?? string.Empty) : string.Empty;
            // Thông tin Company

            //[ADD][08/10/2021][Change company name -> branch name office ]
            //parameter.CompanyName = criteria.Currency == DocumentConstants.CURRENCY_LOCAL ? companyUser?.BunameVn : companyUser?.BunameEn; // Company Name En of user
            parameter.CompanyName = criteria.Currency == DocumentConstants.CURRENCY_LOCAL ? officeUser?.BranchNameVn : officeUser?.BranchNameEn;
            //[END]

            parameter.CompanyDescription = string.Empty;
            parameter.CompanyAddress1 = criteria.Currency == DocumentConstants.CURRENCY_LOCAL ? officeUser?.AddressVn : officeUser?.AddressEn; // Office Address En of user
            parameter.CompanyAddress2 = string.Format(@"Tel: {0}    Fax: {1}", officeUser?.Tel ?? string.Empty, officeUser?.Fax ?? string.Empty); // Tel & Fax of Office user
            parameter.Website = companyUser?.Website; // Website Company of user
            parameter.AccountInfo = string.Empty;
            parameter.Contact = _currentUser;
            parameter.DecimalNo = 3;
            parameter.CurrDecimalNo = 0;

            result = new Crystal
            {
                ReportName = criteria.Currency == DocumentConstants.CURRENCY_LOCAL ? "AirImptArrival.rpt" : criteria.Language == "EN" ? "AirImptArrivalEN.rpt" : "AirImptArrivalOG.rpt",
                AllowPrint = true,
                AllowExport = true
            };

            // Get path link to report
            CrystalEx._apiUrl = apiUrl.Value.Url;
            string folderDownloadReport = CrystalEx.GetLinkDownloadReports();
            var reportName = arrival.ArrivalNo != null ? arrival.ArrivalNo + ".pdf" : "AirImportArrivalNotice_" + transactionRepository.Get(x => x.Id == houseBill.JobId).Select(x => x.JobNo).FirstOrDefault() + ".pdf";
            var _pathReportGenerate = folderDownloadReport + "/" + reportName.Replace("/", "_");
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
                    if (warehouseId != null)
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

        public ProofOfDeliveryViewModel GetProofOfDelivery(Guid hblId)
        {
            ProofOfDeliveryViewModel result = new ProofOfDeliveryViewModel
            {
                HblId = hblId
            };
            var data = detailTransactionRepository.Get(x => x.Id == hblId)?.FirstOrDefault();
            result.DeliveryDate = data?.DeliveryDate;
            result.ReferenceNo = data?.ReferenceNoProof;
            result.Note = data?.Note;
            result.DeliveryPerson = data?.DeliveryPerson;
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
            HandleState result = new HandleState();

            model.TransactionType = DataTypeEx.GetType(model.Type);
            IQueryable<CsArrivalAndDeliveryDefault> data = arrivalDeliveryDefaultRepository.Get(x => x.UserDefault == model.UserDefault && x.TransactionType == model.TransactionType);
            if (data.Count() > 0 && data != null)
            {
                CsArrivalAndDeliveryDefault doDefault = data.FirstOrDefault();
                doDefault.Doheader1 = model.Doheader1;
                doDefault.Doheader2 = model.Doheader2;
                doDefault.Dofooter = model.Dofooter;
                doDefault.UserModified = currentUser.UserID;
                doDefault.DatetimeModified = DateTime.Now;

                result = arrivalDeliveryDefaultRepository.Update(doDefault, x => x.UserDefault == model.UserDefault && x.TransactionType == model.TransactionType);
            }
            else
            {
                CsArrivalAndDeliveryDefault doDefault = new CsArrivalAndDeliveryDefault
                {
                    Doheader1 = model.Doheader1,
                    Doheader2 = model.Doheader2,
                    Dofooter = model.Dofooter,
                    UserDefault = model.UserDefault,
                    TransactionType = model.TransactionType,
                    UserModified = currentUser.UserID,
                    DatetimeModified = DateTime.Now
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

        public HandleState UpdateProofOfDelivery(ProofOfDeliveryViewModel model)
        {
            var detailTransaction = detailTransactionRepository.First(x => x.Id == model.HblId);
            if (detailTransaction == null) return new HandleState(stringLocalizer[LanguageSub.MSG_DATA_NOT_FOUND].Value);
            detailTransaction.ReferenceNoProof = model.ReferenceNo;
            detailTransaction.DeliveryDate = model.DeliveryDate;
            detailTransaction.DeliveryPerson = model.DeliveryPerson;
            detailTransaction.Note = model.Note;
            var result = detailTransactionRepository.Update(detailTransaction, x => x.Id == model.HblId);
            return result;
        }

        public async Task<HandleState> UpdateMultipleProofOfDelivery(ProofOfDeliveryModel model)
        {
            var listHBL = new List<CsTransactionDetail>();

            foreach (var hblId in model.HouseBills)
            {
                var detailTransaction = detailTransactionRepository.First(x => x.Id == hblId);
                if (detailTransaction == null) return new HandleState(stringLocalizer[LanguageSub.MSG_DATA_NOT_FOUND].Value);
                detailTransaction.ReferenceNoProof = model.ReferenceNo;
                detailTransaction.DeliveryDate = model.DeliveryDate;
                detailTransaction.DeliveryPerson = model.DeliveryPerson;
                detailTransaction.DeliveryPerson = model.DeliveryPerson;
                detailTransaction.Note = model.Note;
                listHBL.Add(detailTransaction);
            }

            using (var trans = detailTransactionRepository.DC.Database.BeginTransaction())
            {
                try
                {
                    if (listHBL.Count() > 0)
                    {
                        List<CsStageAssignedModel> listStage = new List<CsStageAssignedModel>();
                        var jobId = listHBL.FirstOrDefault().JobId;

                        foreach (var item in listHBL)
                        {
                            var hs = detailTransactionRepository.Update(item, x => x.Id == item.Id, false);
                            if (hs.Success)
                            {
                                var stage = await stageService.GetStageByType(DocumentConstants.UPDATE_POD);
                                CsStageAssignedModel newItem = new CsStageAssignedModel();

                                newItem.Id = Guid.NewGuid();
                                newItem.StageId = stage.Id;
                                newItem.Status = TermData.Done;
                                newItem.Deadline = DateTime.Now;
                                newItem.MainPersonInCharge = newItem.RealPersonInCharge = currentUser.UserID;
                                newItem.Hblid = item?.Id;
                                newItem.Hblno = item?.Hwbno;
                                newItem.JobId = item.JobId;
                                newItem.Type = DocumentConstants.FROM_SYSTEM;
                                newItem.DatetimeCreated = newItem.DatetimeModified = DateTime.Now;

                                listStage.Add(newItem);
                            }
                        }
                        var hS = await stageAssignedService.AddMultipleStageAssigned(jobId, listStage);
                    }

                    var result = detailTransactionRepository.SubmitChanges();
                    trans.Commit();
                    return result;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    return new HandleState(ex.Message);
                }
                finally
                {
                    trans.Dispose();
                }
            }


        }

        //public SysImage GetFileProofOfDelivery(Guid hblId)
        //{
        //    var result = sysImageRepository.Get(x => x.ObjectId == hblId.ToString()).OrderByDescending(x => x.DateTimeCreated).FirstOrDefault();
        //    return result;
        //}

        //public async Task<ResultHandle> UploadProofOfDeliveryFile(ProofDeliveryFileUploadModel model)
        //{
        //    return await WriteFile(model);
        //}

        private async Task<ResultHandle> WriteFile(ProofDeliveryFileUploadModel model)
        {
            string fileName = "";
            //string folderName = "images";
            string path = this.webUrl.Value.Url;
            try
            {
                var list = new List<SysImage>();
                /* Kiểm tra các thư mục có tồn tại */
                var hs = new HandleState();
                ImageHelper.CreateDirectoryFile(model.FolderName, model.HblId.ToString());
                List<SysImage> resultUrls = new List<SysImage>();
                fileName = model.Files.FileName.Contains("+") ? model.Files.FileName.Replace("+", "_") : model.Files.FileName;
                string objectId = model.HblId.ToString();
                await ImageHelper.SaveFile(fileName, model.FolderName, objectId, model.Files);
                string urlImage = path + "/" + model.FolderName + "/files/" + objectId + "/" + fileName;
                var sysImage = new SysImage
                {
                    Id = Guid.NewGuid(),
                    Url = urlImage,
                    Name = fileName,
                    Folder = model.FolderName ?? "Shipment",
                    ObjectId = model.HblId.ToString(),
                    UserCreated = currentUser.UserName, //admin.
                    UserModified = currentUser.UserName,
                    DateTimeCreated = DateTime.Now,
                    DatetimeModified = DateTime.Now
                };
                resultUrls.Add(sysImage);
                if (!sysImageRepository.Any(x => x.ObjectId == objectId && x.Url == urlImage))
                {
                    list.Add(sysImage);
                }

                if (list.Count > 0)
                {
                    hs = await sysImageRepository.AddAsync(list);
                }
                return new ResultHandle { Data = resultUrls, Status = hs.Success, Message = hs.Message?.ToString() };

            }
            catch (Exception ex)
            {
                return new ResultHandle { Data = null, Status = false, Message = ex.Message };
            }
        }


        //public async Task<HandleState> DeleteFilePOD(Guid id)
        //{
        //    var item = sysImageRepository.Get(x => x.Id == id).FirstOrDefault();
        //    if (item == null) return new HandleState("Not found data");
        //    var result = sysImageRepository.Delete(x => x.Id == id);
        //    if (result.Success)
        //    {
        //        var hs = await ImageHelper.DeleteFile(item.ObjectId + "\\" + item.Name, "Shipment");
        //    }
        //    return result;
        //}


        public Crystal PreviewDeliveryOrder(Guid hblid, string language)
        {
            var detail = detailTransactionRepository.First(x => x.Id == hblid);
            if (detail.DeliveryOrderNo == null) return new Crystal();

            var pic = transactionRepository.Get(x => x.Id == detail.JobId).Select(x => x.PersonIncharge).FirstOrDefault();
            var userInfo = userlevelRepository.Get(x => x.UserId == pic).FirstOrDefault();
            // Company Information
            var companyUser = companyRepo.Get(x => x.Id == userInfo.CompanyId).FirstOrDefault();
            // Office Information
            var officeUser = officeRepo.Get(x => x.Id == userInfo.OfficeId).FirstOrDefault();
            var parameter = new SeaDeliveryCommandParam
            {
                Consignee = "s",
                No = "s",
                CompanyName = language == "EN" ? companyUser?.BunameEn : companyUser?.BunameVn, //Company Name En of user
                CompanyDescription = "Company Description",
                CompanyAddress1 = language == "EN" ? officeUser?.AddressEn : officeUser?.AddressVn, //Office Address En of user
                CompanyAddress2 = string.Format(@"Tel: {0}    Fax: {1}", officeUser?.Tel ?? string.Empty, officeUser?.Fax ?? string.Empty), //Tel & Fax of Office user
                Website = companyUser?.Website, //Website Company of user
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
                unitOfMeasures = cont.UnitOfMeasureId != null ? (unitRepository.Get(x => x.Id == cont.UnitOfMeasureId)?.FirstOrDefault()?.UnitNameEn) : null;
            }

            //Group by Package Type [CR: 15147 - 18/12/2020]
            var grpPkgsType = containers.GroupBy(g => new { g.PackageTypeId }).Select(s => new { PackageTypeId = s.Key.PackageTypeId, PackageQuantitys = s.Select(se => (decimal?)se.PackageQuantity) });
            foreach (var grp in grpPkgsType)
            {
                var pkgsType = unitRepository.Get(x => x.Id == grp.PackageTypeId)?.FirstOrDefault()?.Code; //Unit Code
                var pkgsQty = grp.PackageQuantitys.Sum();
                nopieces += pkgsQty + " " + pkgsType + "\r\n";
            }

            //List Container không có giá trị thì lấy Pkgs Qty, Pkgs Type của Housebill [CR: 15147 - 18/12/2020]
            if (string.IsNullOrEmpty(nopieces))
            {
                var hblPkgsType = unitRepository.Get(x => x.Id == detail.PackageType).FirstOrDefault()?.Code; //Unit Code
                var hblPkgsQty = detail.PackageQty?.ToString();
                nopieces = hblPkgsQty + " " + hblPkgsType;
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
                BillType = detail.ServiceType?.ToUpper(),
                FinalDestination = detail.FinalDestinationPlace?.ToUpper()
            };
            dataSources.Add(item);
            var result = new Crystal
            {
                ReportName = language == "EN" ? "SeaDeliveryCommandEN.rpt" : "SeaDeliveryCommand.rpt",
                AllowPrint = true,
                AllowExport = true
            };

            // Get path link to report
            CrystalEx._apiUrl = apiUrl.Value.Url;
            string folderDownloadReport = CrystalEx.GetLinkDownloadReports();
            var reportName = detail.DeliveryOrderNo != null ? detail.DeliveryOrderNo + ".pdf" : "SeaDeliveryCommand_" + transactionRepository.Get(x => x.Id == detail.JobId).Select(x => x.JobNo).FirstOrDefault() + ".pdf";
            var _pathReportGenerate = folderDownloadReport + "/" + reportName.Replace("/", "_");
            result.PathReportGenerate = _pathReportGenerate;

            result.AddDataSource(dataSources);
            result.FormatType = ExportFormatType.PortableDocFormat;
            result.SetParameter(parameter);
            return result;
        }
        #endregion
    }
}
