using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class CsFclbooking
    {
        public Guid Id { get; set; }
        public Guid BranchId { get; set; }
        public string Code { get; set; }
        public string CustomerId { get; set; }
        public string ConsigneeId { get; set; }
        public Guid PlaceFrom { get; set; }
        public Guid PlaceTo { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string ContainerTypeId { get; set; }
        public decimal? RealWeight { get; set; }
        public string ImExType { get; set; }
        public int? CommodityId { get; set; }
        public string ShipperId { get; set; }
        public int? PoReceipt { get; set; }
        public Guid? PlaceOfLoading { get; set; }
        public Guid? PlaceOfDischarge { get; set; }
        public int? PoDelivery { get; set; }
        public string ReceiptContact { get; set; }
        public string DeliveryContact { get; set; }
        public int? Volume { get; set; }
        public int? RemainQuantity { get; set; }
        public DateTime BookingDate { get; set; }
        public string BookingCustomer { get; set; }
        public string BookingType { get; set; }
        public string OtherRequirement { get; set; }
        public string PaymentTerm { get; set; }
        public string ModeOfTransport { get; set; }
        public string Note { get; set; }
        public string Remark { get; set; }
        public string BookingStatusId { get; set; }
        public string ShipmentStatus { get; set; }
        public Guid? QuotationRouteId { get; set; }
        public string SaleMember { get; set; }
        public string ToApprover { get; set; }
        public string ApprovedUserId { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string ApprovalStatus { get; set; }
        public string ShippingLine { get; set; }
        public bool? IsTrial { get; set; }
        public bool? IsAssociation { get; set; }
        public Guid? BuyingPriceId { get; set; }
        public string BoughtFrom { get; set; }
        public bool? CloseSoa { get; set; }
        public bool? Closed { get; set; }
        public string UserClosed { get; set; }
        public DateTime? DatetimeClosed { get; set; }
        public string ApprovedNote { get; set; }
        public DateTime? ClosingTime { get; set; }
        public string ContNumber { get; set; }
        public string SealNumber { get; set; }
        public string CustomerDebitId { get; set; }
        public bool? IsDraft { get; set; }
        public decimal? SellingPrice { get; set; }
        public string SellingCurrencyId { get; set; }
        public string ApprovedSellingPriceId { get; set; }
        public DateTime? ApprovedSellingDate { get; set; }
        public bool? IsNotInFixedProject { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
        public string DelayedTransportByCreditDate { get; set; }
        public string DelayedTransportByOverDueDate { get; set; }
        public string Sotype { get; set; }
        public string CustomerRouteCode { get; set; }
        public string CustomerBookingNo { get; set; }
    }
}
