using System;
using System.Collections.Generic;

namespace eFMS.API.System.Service.Models
{
    public partial class CsShipmentDetail
    {
        public string JobId { get; set; }
        public string LotNo { get; set; }
        public string Hwbno { get; set; }
        public string ShipperId { get; set; }
        public string SalesManId { get; set; }
        public bool? ShipmentType { get; set; }
        public string BookingCustomsNo { get; set; }
        public string ReceiptAt { get; set; }
        public string Deliveryat { get; set; }
        public DateTime? Etd { get; set; }
        public DateTime? Eta { get; set; }
        public double? Qty { get; set; }
        public string Unit { get; set; }
        public double? GrossWeight { get; set; }
        public double? ChargeWeight { get; set; }
        public double? Cbm { get; set; }
        public double? TotalSellingRate { get; set; }
        public double? TotalBuyingRate { get; set; }
        public double? TotalOtherCredit { get; set; }
        public double? TotalOtherDebit { get; set; }
        public double? TotalProfitShared { get; set; }
        public string Attn { get; set; }
        public string NominationParty { get; set; }
        public string Notes { get; set; }
        public string UserCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string ContainerSize { get; set; }
    }
}
