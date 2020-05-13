using AutoMapper;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Exports;
using eFMS.API.Documentation.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
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

        public CsAirWayBillService(IContextBase<CsAirWayBill> repository, 
            IMapper mapper,
            ICsDimensionDetailService dimensionService,
            ICsShipmentOtherChargeService otherChargeService,
            ICurrentUser currUser,
            IContextBase<CatPlace> catPlace,
            IContextBase<CsTransaction> csTransaction,
            IContextBase<CatPartner> catPartner) : base(repository, mapper)
        {
            dimensionDetailService = dimensionService;
            shipmentOtherChargeService = otherChargeService;
            currentUser = currUser;
            catPlaceRepo = catPlace;
            csTransactionRepo = csTransaction;
            catPartnerRepo = catPartner;
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
            result.RateCharge = masterbill.RateCharge?.ToString();
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
    }
}
