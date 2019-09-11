using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Models.SettlementPayment
{
    public class ShipmentChargeSettlement
    {
        public Guid Id { get; set; }
        public string JobId { get; set; }
        public string HBL { get; set; }
        public string MBL { get; set; }
        public string SettlementNo { get; set; }
        public Guid ChargeId { get; set; }
        public decimal Quantity { get; set; }
        public int UnitID { get; set; }
        public decimal? UnitPrice { get; set; }
        public string Currency { get; set; }
        public decimal? VATRate { get; set; }
        public decimal Amount { get; set; }
        public string PayerID { get; set; }
        public string PaymentObjectID { get; set; }
        public string InvoiceNo { get; set; }
        public string SeriesNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string CustomNo { get; set; }
        public string ContNo { get; set; }
        public string Note { get; set; }
        public bool IsFromShipment { get; set; }
    }
}
