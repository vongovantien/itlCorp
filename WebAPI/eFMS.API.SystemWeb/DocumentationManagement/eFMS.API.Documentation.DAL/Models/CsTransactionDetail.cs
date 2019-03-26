using System;
using System.Collections.Generic;

namespace eFMS.API.Documentation.Service.Models
{
    public partial class CsTransactionDetail
    {
        public Guid Id { get; set; }
        public Guid JobId { get; set; }
        public string Hwbno { get; set; }
        public string Hbltype { get; set; }
        public string CustomerId { get; set; }
        public string SaleManId { get; set; }
        public string ShipperDescription { get; set; }
        public string ShipperId { get; set; }
        public string ConsigneeDescription { get; set; }
        public string ConsigneeId { get; set; }
        public string Commodity { get; set; }
        public string PackageContainer { get; set; }
        public string DesOfGoods { get; set; }
        public string NotifyPartyDescription { get; set; }
        public string NotifyPartyId { get; set; }
        public string CustomsBookingNo { get; set; }
        public string LocalVoyNo { get; set; }
        public string OceanVoyNo { get; set; }
        public short? OriginCountryId { get; set; }
        public string PickupPlace { get; set; }
        public Guid Pol { get; set; }
        public Guid Pod { get; set; }
        public string DeliveryPlace { get; set; }
        public string FinalDestinationPlace { get; set; }
        public string FreightPayment { get; set; }
        public string PlaceFreightPay { get; set; }
        public DateTime? ClosingDate { get; set; }
        public DateTime SailingDate { get; set; }
        public string ForwardingAgentDescription { get; set; }
        public string ForwardingAgentId { get; set; }
        public string GoodsDeliveryDescription { get; set; }
        public string GoodsDeliveryId { get; set; }
        public int? OriginBlnumber { get; set; }
        public string IssueHblplaceAndDate { get; set; }
        public string ReferenceNo { get; set; }
        public string ExportReferenceNo { get; set; }
        public string MoveType { get; set; }
        public string PurchaseOrderNo { get; set; }
        public string ServiceType { get; set; }
        public string ShippingMark { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
        public string InWord { get; set; }
        public string OnBoardStatus { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
    }
}
