using System;
using System.Collections.Generic;

namespace eFMS.API.Setting.Service.Models
{
    public partial class TestHouseBillSeaFclexport
    {
        public string Mblno { get; set; }
        public string Hblno { get; set; }
        public string Hbltype { get; set; }
        public string CustomerId { get; set; }
        public string SaleManId { get; set; }
        public string ShipperId { get; set; }
        public string ConsigneeId { get; set; }
        public string NotifyPartyId { get; set; }
        public string BookingNo { get; set; }
        public string LocalVessel { get; set; }
        public string OceanVessel { get; set; }
        public short PolcountryId { get; set; }
        public string ReceiptPlace { get; set; }
        public Guid LoadingPortId { get; set; }
        public Guid DischargePortId { get; set; }
        public string DeliveryPlace { get; set; }
        public string FinalDestination { get; set; }
        public string FreightPayment { get; set; }
        public string PlaceFreightPay { get; set; }
        public DateTime ClosingDate { get; set; }
        public DateTime SailingDate { get; set; }
        public string FowardingAgentId { get; set; }
        public string GoodsDeliveryId { get; set; }
        public int? OriginBlnumber { get; set; }
        public string IssueHblplaceAndDate { get; set; }
        public string ReferenceNo { get; set; }
        public string ExportReferenceNo { get; set; }
        public string MoveType { get; set; }
        public string PurchaseOrderNo { get; set; }
        public string ServiceType { get; set; }
        public string ShippingMark { get; set; }
        public string InWord { get; set; }
        public string OnBoardStatus { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
    }
}
