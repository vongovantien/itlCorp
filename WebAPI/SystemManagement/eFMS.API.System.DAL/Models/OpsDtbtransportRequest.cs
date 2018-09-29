using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class OpsDtbtransportRequest
    {
        public OpsDtbtransportRequest()
        {
            OpsDtbtransportRequestOrderItemRoute = new HashSet<OpsDtbtransportRequestOrderItemRoute>();
        }

        public Guid Id { get; set; }
        public Guid ResponsibleWorkPlaceId { get; set; }
        public string Code { get; set; }
        public Guid? RouteId { get; set; }
        public int? VehicleId { get; set; }
        public int? DriverId { get; set; }
        public string SupplierId { get; set; }
        public string DriverRole { get; set; }
        public int? RemoocId { get; set; }
        public decimal? WorkingDay { get; set; }
        public string SealNumber { get; set; }
        public DateTime? RequestedForDate { get; set; }
        public string Remark { get; set; }
        public short? Status { get; set; }
        public bool? SentSms { get; set; }
        public string Opsapproved { get; set; }
        public string OpsapprovedStatus { get; set; }
        public DateTime? OpsapprovedDate { get; set; }
        public string Csappoved { get; set; }
        public string CsapprovedStatus { get; set; }
        public DateTime? CsapprovedDate { get; set; }
        public decimal? TotalCod { get; set; }
        public decimal? Mnrcost { get; set; }
        public decimal? FuelCost { get; set; }
        public decimal? TotalCharge { get; set; }
        public decimal? FixedCost { get; set; }
        public decimal? OverheadCost { get; set; }
        public decimal? TotalCost { get; set; }
        public decimal? LenghtGps { get; set; }
        public decimal? TotalLenght { get; set; }
        public decimal? TotalWeight { get; set; }
        public decimal? TotalActualWeight { get; set; }
        public decimal? TotalActualVolume { get; set; }
        public short? WeightUnit { get; set; }
        public decimal? TotalFuelLiter { get; set; }
        public string RefNo { get; set; }
        public string TripSettlementCode { get; set; }
        public bool? CheckPayment { get; set; }
        public bool? IsAccident { get; set; }
        public decimal? FuelConsumption { get; set; }
        public bool? CheckedFuel { get; set; }
        public string UserCheckedFuel { get; set; }
        public DateTime? DatetimeCheckedFuel { get; set; }
        public string ConfirmedOpsman { get; set; }
        public DateTime? DateOpsmanConfirmed { get; set; }
        public string ConfirmationStatus { get; set; }
        public bool? PrintedVehicleRequest { get; set; }
        public DateTime? PrintedDate { get; set; }
        public string PrintedUser { get; set; }
        public string ApprovedCancelBy { get; set; }
        public DateTime? DateApprovedCancel { get; set; }
        public string ApprovedCancelStatus { get; set; }
        public string ApprovedCancelNote { get; set; }
        public string ArisedRequestNote { get; set; }
        public bool? LockedCharge { get; set; }
        public bool? LockedFuel { get; set; }
        public bool? ChangedLength { get; set; }
        public bool? IsHire { get; set; }
        public bool? DeliveredAtBranch { get; set; }
        public string AllowancePaidUser { get; set; }
        public DateTime? AllowancePaidDatetime { get; set; }
        public Guid? FuelPaymentId { get; set; }
        public string Gsanote { get; set; }
        public Guid? TripBuyingRouteId { get; set; }
        public Guid? FclbuyingId { get; set; }
        public Guid? BuyingRouteId { get; set; }
        public DateTime? FinishedTime { get; set; }
        public decimal? LengthInTariff { get; set; }
        public int? StartKm { get; set; }
        public int? EndKm { get; set; }
        public short? TotalLengthCs { get; set; }
        public DateTime? ClosedTime { get; set; }
        public string ClosedUser { get; set; }
        public string ContainerTypeId { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
        public bool? LockedInfo { get; set; }

        public ICollection<OpsDtbtransportRequestOrderItemRoute> OpsDtbtransportRequestOrderItemRoute { get; set; }
    }
}
