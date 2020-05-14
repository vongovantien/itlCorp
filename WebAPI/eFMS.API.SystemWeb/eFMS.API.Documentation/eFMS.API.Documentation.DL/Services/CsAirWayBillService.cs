using AutoMapper;
using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Exports;
using eFMS.API.Documentation.DL.Models.ReportResults;
using eFMS.API.Documentation.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Documentation.DL.Services
{
    public class CsAirWayBillService : RepositoryBase<CsAirWayBill, CsAirWayBillModel>, ICsAirWayBillService
    {
        private readonly ICsDimensionDetailService dimensionDetailService;
        private readonly ICsShipmentOtherChargeService shipmentOtherChargeService;
        private readonly ICurrentUser currentUser;
        readonly IContextBase<CatPlace> catPlaceRepo;
        readonly IContextBase<CsTransaction> csTransactionRepo;
        readonly IContextBase<CatPartner> catPartnerRepo;
        readonly IContextBase<CatCountry> countryRepo;


        public CsAirWayBillService(IContextBase<CsAirWayBill> repository, 
            IMapper mapper,
            ICsDimensionDetailService dimensionService,
            ICsShipmentOtherChargeService otherChargeService,
            ICurrentUser currUser,
            IContextBase<CatPlace> catPlace,
            IContextBase<CsTransaction> csTransaction,
            IContextBase<CatPartner> catPartner,
            IContextBase<CatCountry> catCountry) : base(repository, mapper)
        {
            dimensionDetailService = dimensionService;
            shipmentOtherChargeService = otherChargeService;
            currentUser = currUser;
            catPlaceRepo = catPlace;
            csTransactionRepo = csTransaction;
            catPartnerRepo = catPartner;
            countryRepo = catCountry;
        }

        public CsAirWayBillModel GetBy(Guid jobId)
        {
            var result = Get(x => x.JobId == jobId).FirstOrDefault();
            if (result == null) return null;
            result.DimensionDetails = dimensionDetailService.Get(x => x.AirWayBillId == result.Id).ToList();
            result.OtherCharges = shipmentOtherChargeService.Get(x => x.JobId == jobId).ToList();
            return result;
        }

        public override HandleState Add(CsAirWayBillModel entity)
        {
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var model = mapper.Map<CsAirWayBill>(entity);
                    model.Id = Guid.NewGuid();
                    model.DatetimeModified = DateTime.Now;
                    model.UserModified = currentUser.UserID;
                    var hs = DataContext.Add(model);
                    if (hs.Success)
                    {
                        if(entity.DimensionDetails != null)
                        {
                            entity.DimensionDetails.ForEach(x => {
                                x.UserCreated = currentUser.UserID;
                                x.DatetimeCreated = DateTime.Now;
                                x.Id = Guid.NewGuid();
                                x.AirWayBillId = model.Id;
                            });
                            var hsDimensions = dimensionDetailService.Add(entity.DimensionDetails);
                        }
                        if(entity.OtherCharges != null)
                        {
                            entity.OtherCharges.ForEach(x => {
                                x.UserModified = currentUser.UserID;
                                x.DatetimeModified = DateTime.Now;
                                x.Id = Guid.NewGuid();
                                x.JobId = model.JobId;
                            });
                            var hsOtherCharges = shipmentOtherChargeService.Add(entity.OtherCharges);
                        }
                    }
                    trans.Commit();
                    return hs;
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

        public HandleState Update(CsAirWayBillModel model)
        {
            var bill = mapper.Map<CsAirWayBill>(model);
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var hs = DataContext.Update(bill, x => x.Id == model.Id);
                    if (hs.Success)
                    {
                        if(model.DimensionDetails != null)
                        {
                            var hsdimensions = dimensionDetailService.UpdateAirWayBill(model.DimensionDetails, model.Id);
                        }
                        if(model.OtherCharges != null)
                        {
                            var hsOtherCharges = shipmentOtherChargeService.UpdateOtherChargeMasterBill(model.OtherCharges, model.JobId);
                        }
                    }
                    trans.Commit();
                    return hs;
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

        public AirwayBillExportResult AirwayBillExport(Guid jobId)
        {
            var masterbill = Get(x => x.JobId == jobId).FirstOrDefault();
            if (masterbill == null) return null;
            masterbill.OtherCharges = shipmentOtherChargeService.Get(x => x.JobId == jobId).ToList();
            
            var result = new AirwayBillExportResult();
            result.MawbNo1 = masterbill.Mblno1;
            result.MawbNo2 = masterbill.Mblno2;
            result.MawbNo3 = masterbill.Mblno3;
            var pol = catPlaceRepo.Get(x => x.Id == masterbill.Pol).FirstOrDefault();
            var pod = catPlaceRepo.Get(x => x.Id == masterbill.Pod).FirstOrDefault();
            result.AolCode = pol?.Code;
            result.Shipper = masterbill.ShipperDescription;

            //Airline lấy từ Shipment
            var airlineId = csTransactionRepo.Get(x => x.Id == masterbill.JobId).FirstOrDefault()?.ColoaderId;
            result.AirlineNameEn = catPartnerRepo.Get(x => x.Id == airlineId).FirstOrDefault()?.ShortName; // Name ABBR
            result.Consignee = masterbill.ConsigneeDescription;

            var _airFrieghtDa = string.Empty;
            if (!string.IsNullOrEmpty(masterbill.FreightPayment))
            {
                if (masterbill.FreightPayment == "Sea - Air Difference" || masterbill.FreightPayment == "Prepaid")
                {
                    _airFrieghtDa = "PP IN " + pol?.Code;
                }
                else
                {
                    _airFrieghtDa = "CLL IN " + pod?.Code;
                }
            }
            result.AirFrieghtDa = _airFrieghtDa;

            result.DepartureAirport = pol?.NameEn;
            result.FirstTo = masterbill.FirstCarrierTo;
            result.FirstCarrier = masterbill.FirstCarrierBy;
            result.SecondTo = masterbill.TransitPlaceTo2;
            result.SecondBy = masterbill.TransitPlaceBy2;
            result.Currency = masterbill.CurrencyId;
            result.Dclrca = masterbill.Dclrca;
            result.Dclrcus = masterbill.Dclrcus;
            result.DestinationAirport = pod?.NameEn;
            result.FlightNo = masterbill.FlightNo;
            result.FlightDate = masterbill.FlightDate;
            result.IssuranceAmount = masterbill.IssuranceAmount;
            result.HandingInfo = masterbill.HandingInformation;
            result.Pieces = masterbill.PackageQty;
            result.Gw = masterbill.GrossWeight;
            result.Cw = masterbill.ChargeWeight;
            result.RateCharge = masterbill.RateCharge;
            result.Total = masterbill.Total?.ToString();
            result.DesOfGood = masterbill.DesOfGoods;
            result.VolumeField = masterbill.VolumeField;

            result.PrepaidWt = masterbill.Wtpp;
            result.CollectWt = masterbill.Wtcll;
            result.PrepaidVal = masterbill.Valpp;
            result.CollectVal = masterbill.Valcll;
            result.PrepaidTax = masterbill.Taxpp;
            result.CollectTax = masterbill.Taxcll;
            result.PrepaidDueToCarrier = masterbill.DueCarrierPp;
            result.CollectDueToCarrier = masterbill.DueCarrierCll;

            result.OtherCharges = masterbill.OtherCharges;

            result.PrepaidTotal = masterbill.TotalPp;
            result.CollectTotal = masterbill.TotalCll;
            result.IssueOn = masterbill.IssuedPlace;
            result.IssueDate = masterbill.IssuedDate;
            return result;                 
        }
        public Crystal PreviewAirwayBill(Guid jobId)
        {
            Crystal result = null;
            var data = GetBy(jobId);
            var airWayBills = new List<MasterAirwayBillReport>();
            if (data != null)
            {
                var dataPOD = catPlaceRepo.Get(x => x.Id == data.Pod).FirstOrDefault();
                var dataPOL = catPlaceRepo.Get(x => x.Id == data.Pol).FirstOrDefault();
                var awb = new MasterAirwayBillReport();
                awb.MAWB = data.Mblno1 + data.Mblno2 + data.Mblno3;
                awb.ATTN = ReportUltity.ReplaceNullAddressDescription(data.ShipperDescription)?.ToUpper(); //ShipperName & Address
                awb.Consignee = ReportUltity.ReplaceNullAddressDescription(data.ConsigneeDescription)?.ToUpper(); //Consignee & Address
                awb.AccountingInfo = "FREIGHT " + data.FreightPayment?.ToUpper(); //'FREIGHT ' + Air Freight
                awb.AgentIATACode = "37-3 0118/0000"; //Set Default Value
                if (dataPOL != null)
                {
                    awb.DepartureAirport = dataPOL?.NameEn?.ToUpper(); //POL
                }
                awb.FirstDestination = data.FirstCarrierTo;
                awb.FirstCarrier = data.FirstCarrierBy;
                awb.SecondDestination = data.TransitPlaceTo1;
                awb.SecondCarrier = data.TransitPlaceBy1;
                awb.ThirdDestination = data.TransitPlaceTo2;
                awb.ThirdCarrier = data.TransitPlaceBy2;
                awb.Currency = data.CurrencyId?.ToUpper(); //Currency
                awb.CHGSCode = data.Chgs;
                awb.WTPP = data.Wtpp;
                awb.WTCLL = data.Wtcll;
                awb.ORPP = data.OtherPayment == "PP" ? data.OtherPayment : string.Empty;
                awb.ORCLL = data.OtherPayment == "CLL" ? data.OtherPayment : string.Empty;
                awb.DlvCarriage = data.Dclrca?.ToUpper(); //DCLR-CA
                awb.DlvCustoms = data.Dclrcus?.ToUpper(); //DCLR-CUS
                if (dataPOD != null)
                {
                    var podCountry = countryRepo.Get(x => x.Id == dataPOD.CountryId).FirstOrDefault()?.NameEn;
                    awb.LastDestination = dataPOL?.NameEn + (!string.IsNullOrEmpty(podCountry) ? ", " + podCountry : string.Empty); //AOD - DestinationAirport
                    awb.LastDestination = awb.LastDestination?.ToUpper();
                }
                awb.FlightNo = data.FlightNo;
                awb.FlightDate = data.FlightDate;
                awb.ConnectingFlight = string.Empty; //Để rỗng
                awb.ConnectingFlightDate = null; //Gán null
                awb.insurAmount = data.IssuranceAmount?.ToUpper(); //Issurance Amount
                awb.HandlingInfo = data.HandingInformation?.ToUpper(); //Handing Information
                awb.Notify = data.Notify?.ToUpper(); //Notify
                awb.SCI = string.Empty; //NOT USE
                awb.ReferrenceNo = string.Empty; //NOT USE
                awb.OSI = string.Empty; //NOT USE
                awb.ISSUED = string.Empty; //NOT USE
                awb.ConsigneeID = data.ConsigneeId; //NOT USE
                awb.ICASNC = string.Empty; //NOT USE
                awb.NoPieces = data.PackageQty != null ? data.PackageQty.ToString() : string.Empty; //Số kiện (Pieces)
                awb.GrossWeight = data.GrossWeight ?? 0; //GrossWeight
                awb.GrwDecimal = 2; //NOT USE
                awb.Wlbs = data.KgIb?.ToUpper(); //KgIb
                awb.RateClass = data.Rclass;
                awb.ItemNo = data.ComItemNo?.ToUpper(); //ComItemNo - Commodity Item no
                awb.WChargeable = data.ChargeWeight ?? 0; //CW
                awb.ChWDecimal = 2; //NOT USE
                awb.Rchge = data.RateCharge != null ? data.RateCharge.ToString() : string.Empty; //RateCharge
                awb.Ttal = data.Total != null ? data.Total.ToString() : string.Empty;
                awb.Description = data.DesOfGoods?.ToUpper(); //Natural and Quality Goods
                awb.WghtPP = data.Wtpp?.ToUpper(); //WT (prepaid)
                awb.WghtCC = data.Wtcll?.ToUpper(); //WT (Collect)
                awb.ValChPP = string.Empty; //NOT USE
                awb.ValChCC = string.Empty; //NOT USE
                awb.TxPP = string.Empty; //NOT USE
                awb.TxCC = string.Empty; //NOT USE
                awb.OrchW = data.OtherCharge?.ToUpper(); //Other Charge



            }
        }
    }
}
