using System;

namespace eFMS.API.Accounting.DL.Models
{
    public class ChargeShipmentModel
    {
        public Guid ID { get; set; }
        public string ChargeCode { get; set; }
        public string ChargeName { get; set; }
        public string JobId { get; set; }
        public string HBL { get; set; }
        public string MBL { get; set; }
        public string CustomNo { get; set; }
        public string Type { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime? ServiceDate { get; set; }
        public string Note { get; set; }
        public string Currency { get; set; }
        public string CurrencyToLocal { get; set; }
        public string CurrencyToUSD { get; set; }
        public Nullable<decimal> Debit { get; set; }
        public Nullable<decimal> Credit { get; set; }
        public decimal AmountDebitLocal { get; set; }
        public decimal AmountCreditLocal { get; set; }
        public decimal AmountDebitUSD { get; set; }
        public decimal AmountCreditUSD { get; set; }

        public string SOANo { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal VATRate { get; set; }
        public DateTime? DatetimeModifiedSurcharge { get; set; }
        public string CDNote { get; set; }
        public string PIC { get; set; }
        public bool IsSynced { get; set; }
        public string SyncedFromBy { get; set; }

    }
}
