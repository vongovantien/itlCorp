using System;


namespace eFMS.API.Accounting.DL.Models.Receipt
{
    public class ReceiptInvoiceModel
    {
        public string InvoiceNo { get; set; }
        public string SerieNo { get; set; }
        public string Type { get; set; }
        public string PartnerName { get; set; }
        public string TaxCode { get; set; }
        public decimal UnpaidAmount { get; set; }
        public string Currency { get; set; }
        public decimal? PaidAmount { get; set; }
        public string InvoiceBalance { get; set; }
        public decimal? RefAmount { get; set; }
        public string RefCurrency { get; set; }
        public string PaymentStatus { get; set; }
        public DateTime? BillingDate{ get; set; }
        public DateTime? InvoiceDate{ get; set; }
        public DateTime? Note{ get; set; }
    }
}
