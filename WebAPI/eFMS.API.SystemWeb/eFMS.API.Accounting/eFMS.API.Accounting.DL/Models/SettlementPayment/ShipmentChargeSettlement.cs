using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models.SettlementPayment
{
    public class ShipmentChargeSettlement
    {
        public Guid Id { get; set; }
        public string JobId { get; set; }
        public string HBL { get; set; }
        public string MBL { get; set; }
        public string ChargeCode { get; set; }
        public Guid Hblid { get; set; }
        public string Type { get; set; }
        public Guid ChargeId { get; set; }
        public string ChargeName { get; set; }
        public decimal Quantity { get; set; }
        public short UnitId { get; set; }
        public string UnitName { get; set; }
        public decimal? UnitPrice { get; set; }
        public string CurrencyId { get; set; }
        public decimal? Vatrate { get; set; }
        public decimal Total { get; set; }
        public string PayerId { get; set; }
        public string Payer { get; set; }
        public string ObjectBePaid { get; set; }
        public string PaymentObjectId { get; set; }
        public string OBHPartnerName { get; set; }
        public string Notes { get; set; }
        public string SettlementCode { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string SeriesNo { get; set; }
        public string PaymentRequestType { get; set; }
        public string ClearanceNo { get; set; }
        public string ContNo { get; set; }
        public bool? Soaclosed { get; set; }
        public bool? Cdclosed { get; set; }
        public string CreditNo { get; set; }
        public string DebitNo { get; set; }
        public string Soano { get; set; }
        public bool? IsFromShipment { get; set; }
        public string PaySoano { get; set; }
        public string TypeOfFee { get; set; }
        public string AdvanceNo { get; set; }
        public string TypeService { get; set; } // OPS | DOC
        public Guid ShipmentId { get; set; }
        public Guid? ChargeGroup { get; set; }

    }
}
