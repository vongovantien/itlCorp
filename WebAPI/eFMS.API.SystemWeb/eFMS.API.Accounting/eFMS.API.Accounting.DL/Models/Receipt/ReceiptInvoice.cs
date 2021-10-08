using System;
using System.Collections.Generic;

namespace eFMS.API.Accounting.DL.Models.Receipt
{
    public class ReceiptInvoiceModel: CustomerDebitCreditModel
    {
        public Guid? Id { get; set; }
        public Guid? PaymentId { get; set; }
        public string Notes { get; set; }
        public decimal? PaidAmountVnd { get; set; }
        public decimal? PaidAmountUsd { get; set; }
        public decimal? RemainUsd { get; set; }
        public decimal? RemainVnd { get; set; }

        public string JobNo { get; set; }
        public string Mbl { get; set; }
        public string Hbl { get; set; }
        public Guid? Hblid { get; set; }
        public List<string> CreditNos { get; set; }
        public bool? IsValid { get; set; }
    }

    public class ProcessClearInvoiceModel
    {
        public List<ReceiptInvoiceModel> Invoices { get; set; }
        public decimal CusAdvanceAmountVnd { get; set; }
        public decimal CusAdvanceAmountUsd { get; set; }
    }
}
