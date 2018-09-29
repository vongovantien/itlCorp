using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class OpsFclmasterTransportRequest
    {
        public OpsFclmasterTransportRequest()
        {
            OpsFcltransportRequest = new HashSet<OpsFcltransportRequest>();
        }

        public Guid Id { get; set; }
        public Guid PlaceFromId { get; set; }
        public Guid PlaceToId { get; set; }
        public Guid BookingDetailId { get; set; }
        public string Code { get; set; }
        public string Awb { get; set; }
        public string LonghualShipmentCode { get; set; }
        public decimal? WorkingDay { get; set; }
        public string ContNumber { get; set; }
        public string SealNumber { get; set; }
        public DateTime? RequestedForDate { get; set; }
        public string Remark { get; set; }
        public short? Status { get; set; }
        public bool? IsAssociationQuotation { get; set; }
        public bool? CustomerDenied { get; set; }
        public decimal? Mnrcost { get; set; }
        public decimal? FuelCost { get; set; }
        public decimal? TotalCharge { get; set; }
        public decimal? FixedCost { get; set; }
        public decimal? OverheadCost { get; set; }
        public decimal? TotalRevenue { get; set; }
        public decimal? Gpslenght { get; set; }
        public decimal? TotalLenght { get; set; }
        public decimal? TotalEmptyContLength { get; set; }
        public decimal TotalWeight { get; set; }
        public decimal? TotalWeightReal { get; set; }
        public decimal? TotalFuelLiter { get; set; }
        public bool? CheckPayment { get; set; }
        public bool? ClosedSoa { get; set; }
        public Guid? Soaid { get; set; }
        public bool? ExportedSoa { get; set; }
        public decimal? FuelConsumption { get; set; }
        public string ConfirmedOpsman { get; set; }
        public DateTime? DateOpsmanConfirmed { get; set; }
        public string ConfirmationStatus { get; set; }
        public string CancelReason { get; set; }
        public string ApprovedCancelBy { get; set; }
        public DateTime? DateApprovedCancel { get; set; }
        public string ApprovedCancelNote { get; set; }
        public bool? LockedCharge { get; set; }
        public bool? LockedFuel { get; set; }
        public bool? ChangedLength { get; set; }
        public decimal? SellingPrice { get; set; }
        public decimal? AdjustedPrice { get; set; }
        public string SellingCurrencyId { get; set; }
        public string AdjustmentRequestor { get; set; }
        public string AdjustedNotes { get; set; }
        public DateTime? AdjustmentRequestDate { get; set; }
        public string ApprovedAdjustmentBy { get; set; }
        public string ApprovedAdjustmentNote { get; set; }
        public string ApprovedAdjustmentStatus { get; set; }
        public DateTime? ApprovedAdjustmentDate { get; set; }
        public decimal? TotalProfit { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
        public string ApprovedCancelStatus { get; set; }
        public bool? Soaclosed { get; set; }
        public string Soano { get; set; }
        public string UnlockedSoasaleMan { get; set; }
        public string UnlockedSoasaleManStatus { get; set; }
        public DateTime? UnlockedSoasaleManDate { get; set; }
        public string UnlockedSoadirector { get; set; }
        public string UnlockedSoadirectorStatus { get; set; }
        public DateTime? UnlockedSoadirectorDate { get; set; }
        public string AdjustedSoauser { get; set; }
        public DateTime? AdjustedSoadate { get; set; }
        public string SoaadjustmentRequestor { get; set; }
        public DateTime? SoaadjustmentRequestedDate { get; set; }
        public string SoaadjustmentReason { get; set; }
        public bool? BillingGenerated { get; set; }
        public string BillingNo { get; set; }
        public decimal? Codvalue { get; set; }
        public string DeliveryStatus { get; set; }
        public string FailedDeliveryReason { get; set; }
        public string FailedDeliveryDueTo { get; set; }
        public DateTime? PodreceivedDate { get; set; }
        public DateTime? PodhanoverRequestDate { get; set; }
        public DateTime? PodreturnedDate { get; set; }
        public int? PodleadTime { get; set; }

        public ICollection<OpsFcltransportRequest> OpsFcltransportRequest { get; set; }
    }
}
