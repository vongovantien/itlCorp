using AutoMapper;
using AutoMapper.QueryableExtensions;
using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
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
            IContextBase<CsMawbcontainer> containerRepo
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
            var parameter = new SeaDeliveryCommandParam {
                Consignee = "s",
                No = "s",
                CompanyName = "Công ty IndoTrans",
                CompanyDescription = "Company Description",
                CompanyAddress1 = "52 Trường Sơn, Phường 2, Tân Bình",
                CompanyAddress2 = "52 Trường Sơn, Phường 2, Tân Bình",
                Website = "itlvn.com.vn",
                MAWB = detail.Mawb,
                Contact = "admin",
                DecimalNo = 2
            };
            var polName= placeRepository.Get(x => x.Id == detail.Pol).FirstOrDefault().NameEn;
            var podName = placeRepository.Get(x => x.Id == detail.Pod).FirstOrDefault().NameEn;
            var dataSources = new List<SeaDeliveryCommandResult>();
            var containers = containerRepository.Get(x => x.Hblid == hblid);
            foreach(var cont in containers)
            {
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
                    TotalPackages = cont.Quantity + "X" + unitRepository.Get(x => x.Id ==  cont.ContainerTypeId)?.FirstOrDefault()?.UnitNameEn,
                    NoPieces = cont.PackageQuantity + " " + unitRepository.Get(x => x.Id == cont.PackageTypeId)?.FirstOrDefault()?.UnitNameEn,
                    GrossWeight = (decimal)cont.Gw,
                    Unit = unitRepository.Get(x => x.Id == cont.UnitOfMeasureId)?.FirstOrDefault()?.UnitNameEn,
                    CBM = (decimal)cont.Cbm,
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
    }
}
