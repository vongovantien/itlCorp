using System;
using System.Collections.Generic;
using System.Text;

namespace FMS.API.ReportData.Models.Accounting
{
    public class AccountingCustomerPaymentExport
    {
        public string PartnerCode { get; set; }
        public string ParentCode { get; set; }
        public string PartnerName { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string BillingRefNo { get; set; }
        public DateTime? BillingDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal? UnpaidAmountInv { get; set; }
        public decimal? UnpaidAmountInvUsd { get; set; }
        public decimal? UnpaidAmountOBH { get; set; }
        public decimal? UnpaidAmountOBHUsd { get; set; }
        public decimal? PaidAmount { get; set; }
        public decimal? PaidAmountOBH { get; set; }
        public decimal? PaidAmountUsd { get; set; }
        public decimal? PaidAmountOBHUsd { get; set; }
        public string JobNo { get; set; }
        public string HBL { get; set; }
        public string MBL { get; set; }
        public string CustomNo { get; set; }
        public string Salesman { get; set; }
        public string Creator { get; set; }
        public List<AccountingReceiptDetail> receiptDetail;
    }
    
    public class AccountingReceiptDetail
    {
        public string PaymentRefNo { get; set; }
        public DateTime? PaymentDate { get; set; }
        public decimal? PaidAmount { get; set; }
        public decimal? PaidAmountOBH { get; set; }
    }
}
