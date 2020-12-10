using System;
using System.Collections.Generic;

namespace eFMS.API.Accounting.DL.Models.Receipt
{
    public class ReceiptInvoiceModel
    {
        public int Index { get; set; }
        public Guid? PaymentId { get; set; }
        public string InvoiceId { get; set; }
        public string InvoiceNo { get; set; }
        public string SerieNo { get; set; }
        public string Type { get; set; }
        public string PartnerName { get; set; }
        public string TaxCode { get; set; }
        public string Currency { get; set; }
        public decimal? RefAmount { get; set; }
        public string RefCurrency { get; set; }
        public string PaymentStatus { get; set; }
        public DateTime? BillingDate { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string Note { get; set; }

        public decimal UnpaidAmount { get; set; } // số tiền còn lại của invoice
        public decimal? ReceiptExcUnpaidAmount { get; set; } // tiền còn lại của invoice -> quy đổi theo tiền tệ của receipt
        public decimal? PaidAmount { get; set; } // số tiền thu của invoice
        public decimal? InvoiceBalance { get; set; } // số tiền còn lại của invoice
    }

    public class ProcessClearInvoiceModel
    {
        public List<ReceiptInvoiceModel> Invoices { get; set; }
        public decimal Balance { get; set; }
    }
}
