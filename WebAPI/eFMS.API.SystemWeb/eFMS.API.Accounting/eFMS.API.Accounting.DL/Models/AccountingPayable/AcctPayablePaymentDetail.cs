using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models.AccountingPayable
{
    public class AcctPayablePaymentDetailModel
    {
        public string RefId { get; set; }
        public string PartnerId { get; set; }
        public string VoucherNo { get; set; }
        public DateTime? VoucherDate { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string ReferenceNo { get; set; }
        public string BillingNo { get; set; }
        public string BillingType { get; set; }
        public string Currency { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? TotalAmountVnd { get; set; }
        public decimal? TotalAmountUsd { get; set; }
        public decimal? PaidAmountOrg { get; set; }
        public decimal? PaidAmountVnd { get; set; }
        public decimal? PaidAmountUsd { get; set; }
        public decimal? OrgRemainAmount { get; set; }
        public decimal? RemainAmountVnd { get; set; }
        public decimal? RemainAmountUsd { get; set; }
        public string Status { get; set; }
        public string TransactionType { get; set; }
        public decimal? PaymentTerm { get; set; }
        public DateTime? PaymentDueDate { get; set; }
        public string Description { get; set; }
        /// <summary>Type nhận dạng group AP </summary>
        public string InRangeType { get; set; }

        public string PaymentNo { get; set; }
        public string PaymentReferenceNo { get; set; }
        public string PaymentType { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string CurrencyPayment { get; set; }
        public string StatusPayment { get; set; }
        public decimal? PaymentAmount { get; set; }
        public decimal? PaymentAmountVnd { get; set; }
        public decimal? PaymentAmountUsd { get; set; }
        public decimal? PaymentRemainAmount { get; set; }
        public decimal? PaymentRemainAmountVnd { get; set; }
        public decimal? PaymentRemainAmountUsd { get; set; }
        public DateTime? PaymentDatetimeCreated { get; set; }
        public string PaymentAcctId { get; set; }
    }
}
