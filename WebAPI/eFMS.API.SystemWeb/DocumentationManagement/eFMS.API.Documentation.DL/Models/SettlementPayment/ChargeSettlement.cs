using System;

namespace eFMS.API.Documentation.DL.Models.SettlementPayment
{
    public class ChargeSettlement
    {
        public Guid Id { get; set; }
        public string SettlementNo { get; set; }
        public Guid ChargeId { get; set; }
        public string ChargeName { get; set; }
        public decimal Quantity { get; set; }
        public int UnitID { get; set; }
        public string UnitName { get; set; }
        public decimal? UnitPrice { get; set; }
        public string Currency { get; set; }
        public decimal? VATRate { get; set; }
        public decimal Amount { get; set; }
        public string PayerID { get; set; }
        public string Payer { get; set; }
        public string PaymentObjectID { get; set; }
        public string OBHPartner { get; set; }
        public string InvoiceNo { get; set; }
        public string SeriesNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string CustomNo { get; set; }
        public string ContNo { get; set; }
        public string Note { get; set; }
    }
}
