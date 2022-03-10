using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models.AccountingPayable
{
    public class AcctPayablePaymentExport
    {
        public string AcctRefNo { get; set; }
        public string TransactionType { get; set; }
        public DateTime? AcctDate { get; set; }
        public string AccountNo { get; set; }
        public string PartnerName { get; set; }
        public string BillingNo { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime? Invoicedate { get; set; }
        public string DocNo { get; set; }
        public decimal? BeginAmount { get; set; }
        public string OriginCurrency { get; set; }
        public decimal? BeginAmountVND { get; set; }
        public decimal? PaymentTerm { get; set; }
        public DateTime? PaymentDueDate { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }

        public List<AcctPayablePaymentDetail> PaymentDetails { get; set; }
    }

    public class AcctPayablePaymentDetail
    {
        public string PaymentRefNo { get; set; }
        public DateTime? PaymentDatetimeCreated { get; set; }
        public DateTime? PaymentDate { get; set; }
        public decimal? OrgPaidAmount { get; set; }
        public decimal? OriginRemainAmount { get; set; }
        public decimal? PaidAmountVND { get; set; }
        public decimal? RemainAmountVND { get; set; }
        public string OriginCurrency { get; set; }
    }
}
