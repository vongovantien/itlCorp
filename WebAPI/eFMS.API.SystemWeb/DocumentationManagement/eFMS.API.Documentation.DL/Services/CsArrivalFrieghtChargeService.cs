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
using System.Text;
using System.Web;

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
        IContextBase<CatPartner> partnerRepositoty;
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
                var tempdata = freightCharges.Join(charges, x => x.ChargeId, y => y.Id, (x, y) => new { ArrivalFrieghtCharge = x, ChargeCode = y.Code })
                    .Join(units, x => x.ArrivalFrieghtCharge.UnitId, y => y.Id, (x, y) => new { CsArrivalFrieghtCharge = x, UnitName = y.Code });
                foreach (var item in tempdata)
                {
                    var charge = item.CsArrivalFrieghtCharge.ArrivalFrieghtCharge;
                    charge.CurrencyName = charge.CurrencyId;
                    charge.ChargeName = item.CsArrivalFrieghtCharge.ChargeCode;
                    charge.UnitName = item.UnitName;
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
            var _currentUser = currentUser.UserID;
            var listCharge = new List<SeaArrivalNotesReport>();

            var arrival = GetArrival(criteria.HblId, string.Empty);
            var houserBill = houseBills.GetById(criteria.HblId);
            if (arrival != null && houserBill != null)
            {
                var polName = placeRepository.Get(x => x.Id == houserBill.Pol).FirstOrDefault()?.NameEn;
                var podName = placeRepository.Get(x => x.Id == houserBill.Pod).FirstOrDefault()?.NameEn;

                var _arrivalHeader = ReplaceHtmlBaseForPreviewReport(arrival.ArrivalHeader);
                var _arrivalFooter = ReplaceHtmlBaseForPreviewReport(arrival.ArrivalFooter);

                if (arrival.CsArrivalFrieghtCharges.Count > 0)
                {
                    foreach (var frieght in arrival.CsArrivalFrieghtCharges)
                    {
                        var charge = new SeaArrivalNotesReport();
                        charge.HWBNO = houserBill.Hwbno;
                        charge.ArrivalNo = arrival.ArrivalNo;
                        charge.ReferrenceNo = houserBill.ReferenceNo;
                        charge.ISSUED = DateTime.Now.ToString("dd/MM/yyyy");//Issued Date
                        charge.ATTN = houserBill.ShipperDescription;//Shipper
                        charge.Consignee = houserBill.ConsigneeDescription;
                        charge.Notify = houserBill.NotifyPartyDescription;
                        charge.HandlingInfo = string.Empty;
                        charge.ExecutedOn = string.Empty;
                        charge.OceanVessel = houserBill.OceanVessel;
                        charge.OSI = _arrivalHeader;//Header of arrival
                        if (houserBill.Eta != null)
                        {
                            charge.FlightDate = houserBill.Eta.Value; //ETA
                        }
                        charge.DateConfirm = DateTime.Now;
                        charge.DatePackage = DateTime.Now;
                        charge.LocalVessel = houserBill.OceanVessel;//Ocean Vessel of HBL
                        charge.ContSealNo = houserBill.OceanVoyNo;// Ocean Voy No of HBL
                        charge.ForCarrier = string.Empty;
                        charge.DepartureAirport = polName;//POL of HBL
                        charge.PortofDischarge = podName;//POD of HBL
                        charge.PlaceDelivery = podName;//POD of HBL
                        charge.ArrivalNote = houserBill.Remark;//Remark of HBL

                        charge.ShippingMarkImport = houserBill.ShippingMark;//ShippingMark of HBL
                        
                        charge.TotalPackages = houserBill.PackageContainer;// Detail container & package                    
                        charge.Description = houserBill.DesOfGoods;// Description of goods (Description)
                        charge.NoPieces = houserBill.PackageQty.ToString();
                        charge.GrossWeight = houserBill.GrossWeight != null ? houserBill.GrossWeight.Value : 0; //GrossWeight of container
                        charge.CBM = houserBill.Cbm != null ? houserBill.Cbm.Value : 0; //CBM of container
                        charge.Unit = "KGS"; //Đang gán cứng

                        charge.blnShow = frieght.IsShow != null ? frieght.IsShow.Value : false; //isShow of charge arrival
                        charge.blnStick = frieght.IsTick != null ? frieght.IsTick.Value : false;//isStick of charge arrival
                        charge.blnRoot = frieght.IsFull != null ? frieght.IsFull.Value : false; //isRoot of charge arrival
                        charge.FreightCharges = chargeRepository.Get(x => x.Id == frieght.ChargeId).FirstOrDefault()?.ChargeNameEn;//Charge name of charge arrival
                        charge.Qty = frieght.Quantity != null ? frieght.Quantity.Value : 0;//Quantity of charge arrival
                        charge.QUnit = frieght.UnitName;//Unit name of charge arrival
                        charge.TotalValue = frieght.UnitPrice != null ? frieght.UnitPrice.Value : 0;//Unit price of charge arrival
                        charge.Curr = frieght.CurrencyId; //Currency of charge arrival
                        charge.VAT = frieght.Vatrate != null ? frieght.Vatrate.Value : 0; //VAT of charge arrival
                        charge.Notes = frieght.Notes;//Note of charge arrival
                        charge.ArrivalFooterNoitice = _arrivalFooter;//Footer of arrival
                        charge.SeaFCL = true; //Đang gán cứng lấy hàng nguyên công
                        charge.MaskNos = "MaskNos";
                        charge.DlvCustoms = "DlvCustoms";
                        charge.insurAmount = "insurAmount";
                        charge.BillType = houserBill.Hbltype; // House Bill of Lading Type
                        charge.DOPickup = DateTime.Now;
                        charge.ExVND = frieght.ExchangeRate != null ? frieght.ExchangeRate.Value : 0;
                        charge.DecimalSymbol = ",";//Dấu phân cách phần ngàn
                        charge.DigitSymbol = ".";//Dấu phân cách phần thập phân
                        charge.DecimalNo = 2;
                        charge.CurrDecimalNo = 2;

                        listCharge.Add(charge);
                    }
                }
                else
                {
                    var charge = new SeaArrivalNotesReport();
                    charge.HWBNO = houserBill.Hwbno;
                    charge.ArrivalNo = arrival.ArrivalNo;
                    charge.ReferrenceNo = houserBill.ReferenceNo;
                    charge.ISSUED = DateTime.Now.ToString("dd/MM/yyyy");//Issued Date
                    charge.ATTN = houserBill.ShipperDescription;//Shipper
                    charge.Consignee = houserBill.ConsigneeDescription;
                    charge.Notify = houserBill.NotifyPartyDescription;
                    charge.HandlingInfo = string.Empty;
                    charge.ExecutedOn = string.Empty;
                    charge.OceanVessel = houserBill.OceanVessel;
                    charge.OSI = _arrivalHeader;//Header of arrival
                    if (houserBill.Eta != null)
                    {
                        charge.FlightDate = houserBill.Eta.Value; //ETA
                    }
                    charge.DateConfirm = DateTime.Now;
                    charge.DatePackage = DateTime.Now;
                    charge.LocalVessel = houserBill.OceanVessel;//Ocean Vessel of HBL
                    charge.ContSealNo = houserBill.OceanVoyNo;// Ocean Voy No of HBL
                    charge.ForCarrier = string.Empty;
                    charge.DepartureAirport = polName;//POL of HBL
                    charge.PortofDischarge = podName;//POD of HBL
                    charge.PlaceDelivery = podName;//POD of HBL
                    charge.ArrivalNote = houserBill.Remark;//Remark of HBL

                    charge.ShippingMarkImport = houserBill.ShippingMark;//ShippingMark of HBL

                    charge.TotalPackages = houserBill.PackageContainer;// Detail container & package                    
                    charge.Description = houserBill.DesOfGoods;// Description of goods (Description)
                    charge.NoPieces = houserBill.PackageQty.ToString();
                    charge.GrossWeight = houserBill.GrossWeight != null ? houserBill.GrossWeight.Value : 0; //GrossWeight of container
                    charge.CBM = houserBill.Cbm != null ? houserBill.Cbm.Value : 0; //CBM of container
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
                    charge.ArrivalFooterNoitice = _arrivalFooter;//Footer of arrival
                    charge.SeaFCL = true; //Đang gán cứng lấy hàng nguyên công
                    charge.MaskNos = "MaskNos";
                    charge.DlvCustoms = "DlvCustoms";
                    charge.insurAmount = "insurAmount";
                    charge.BillType = houserBill.Hbltype; // House Bill of Lading Type
                    charge.DOPickup = DateTime.Now;
                    charge.ExVND = 0;
                    charge.DecimalSymbol = ",";//Dấu phân cách phần ngàn
                    charge.DigitSymbol = ".";//Dấu phân cách phần thập phân
                    charge.DecimalNo = 2;
                    charge.CurrDecimalNo = 2;

                    listCharge.Add(charge);
                }
            }

            var parameter = new SeaArrivalNotesReportParams();
            parameter.No = string.Empty;
            parameter.ShipperName = string.Empty;
            parameter.CompanyName = Constants.COMPANY_NAME;
            parameter.CompanyDescription = string.Empty;
            parameter.CompanyAddress1 = Constants.COMPANY_ADDRESS1;
            parameter.CompanyAddress2 = "Tel‎: (‎84‎-‎8‎) ‎3948 6888  Fax‎: +‎84 8 38488 570‎";
            parameter.Website = Constants.COMPANY_WEBSITE;
            parameter.MAWB = houserBill != null ? houserBill.Mawb: string.Empty;
            parameter.Contact = _currentUser;
            parameter.DecimalNo = 0;
            parameter.CurrDecimalNo = 0;

            result = new Crystal
            {
                ReportName = criteria.Currency == Constants.CURRENCY_LOCAL ? "SeaArrivalNotes.rpt" : "SeaArrivalNotesOG.rpt",
                AllowPrint = true,
                AllowExport = true
            };
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
                result.Doheader1 = data.DosentTo1;
                result.Doheader2 = data.DosentTo2;
                result.Dofooter = data.Dofooter;
                result.DeliveryOrderNo = data.DeliveryOrderNo;
                result.DeliveryOrderPrintedDate = data.DeliveryOrderPrintedDate;
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
                CompanyName = "Công ty IndoTrans",
                CompanyDescription = "Company Description",
                CompanyAddress1 = "52 Trường Sơn, Phường 2, Tân Bình",
                CompanyAddress2 = "52 Trường Sơn, Phường 2, Tân Bình",
                Website = "itlvn.com.vn",
                MAWB = detail.Mawb,
                Contact = currentUser.UserName,
                DecimalNo = 2
            };
            var polName = placeRepository.Get(x => x.Id == detail.Pol).FirstOrDefault().NameEn;
            var podName = placeRepository.Get(x => x.Id == detail.Pod).FirstOrDefault().NameEn;
            var dataSources = new List<SeaDeliveryCommandResult>();
            var containers = containerRepository.Get(x => x.Hblid == hblid);
            foreach (var cont in containers)
            {
                string totalPackages = cont.ContainerTypeId != null ? (cont.Quantity + "X" + unitRepository.Get(x => x.Id == cont.ContainerTypeId)?.FirstOrDefault()?.UnitNameEn) : null;
                string nopieces = cont.PackageTypeId != null ? (cont.PackageQuantity + " " + unitRepository.Get(x => x.Id == cont.PackageTypeId)?.FirstOrDefault()?.UnitNameEn) : null;
                string unitOfMeasures = cont.UnitOfMeasureId != null ? (unitRepository.Get(x => x.Id == cont.UnitOfMeasureId)?.FirstOrDefault()?.UnitNameEn) : null;
                var item = new SeaDeliveryCommandResult
                {
                    DONo = detail.DeliveryOrderNo,
                    LocalVessel = detail.LocalVessel,
                    Consignee = detail.ConsigneeDescription,
                    OceanVessel = detail.OceanVessel,
                    DepartureAirport = polName,
                    PortofDischarge = podName,
                    PlaceDelivery = podName,
                    HWBNO = detail.Hwbno,
                    ShippingMarkImport = detail.ShippingMark,
                    SpecialNote = "",
                    Description = cont.Description,
                    ContSealNo = cont.SealNo, //continue
                    TotalPackages = totalPackages,
                    NoPieces = nopieces,
                    GrossWeight = cont.Gw,
                    Unit = unitOfMeasures,
                    CBM = cont.Cbm != null?cont.Cbm: 0,
                    DeliveryOrderNote = detail.Dofooter,
                    FirstDestination = detail.DosentTo1,
                    SecondDestination = detail.DosentTo2,
                    ArrivalNote = detail.ArrivalNo,
                    FlightDate = detail.Eta
                };
                dataSources.Add(item);
            }
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

        private string ReplaceHtmlBaseForPreviewReport(string html)
        {
            if (string.IsNullOrEmpty(html)) return html;
            if (html.Contains("<strong>"))
            {
                html = html.Replace("<strong>", "<b>");
            }
            if (html.Contains("</strong>"))
            {
                html = html.Replace("</strong>", "</b>");
            }
            if (html.Contains("<em>"))
            {
                html = html.Replace("<em>", "<i>");
            }
            if (html.Contains("</em>"))
            {
                html = html.Replace("</em>", "</i>");
            }
            return html;
        }
    }
}
