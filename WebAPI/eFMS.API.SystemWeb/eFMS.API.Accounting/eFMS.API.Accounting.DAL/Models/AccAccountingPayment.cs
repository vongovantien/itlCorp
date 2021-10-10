using System;
using System.Collections.Generic;

namespace eFMS.API.Accounting.Service.Models
{
    public partial class AccAccountingPayment
    {
        public Guid Id { get; set; }
        public string RefId { get; set; }
        public string PaymentNo { get; set; }
        public decimal? PaymentAmount { get; set; }
        public decimal? Balance { get; set; }
        public string CurrencyId { get; set; }
        public DateTime? PaidDate { get; set; }
        public string PaymentType { get; set; }
        public string Type { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public short? GroupId { get; set; }
        public int? DepartmentId { get; set; }
        public Guid? OfficeId { get; set; }
        public Guid? CompanyId { get; set; }
        public decimal? ExchangeRate { get; set; }
        public string PaymentMethod { get; set; }
        public Guid? ReceiptId { get; set; }
        public string BillingRefNo { get; set; }
        public decimal? RefAmount { get; set; }
        public string RefCurrency { get; set; }
        public string Note { get; set; }
        public decimal? PaymentAmountVnd { get; set; }
        public decimal? PaymentAmountUsd { get; set; }
        public int? DeptInvoiceId { get; set; }
        public Guid? OfficeInvoiceId { get; set; }
        public Guid? CompanyInvoiceId { get; set; }
        public decimal? UnpaidPaymentAmountUsd { get; set; }
        public decimal? UnpaidPaymentAmountVnd { get; set; }
        public decimal? BalanceVnd { get; set; }
        public decimal? BalanceUsd { get; set; }
        public string InvoiceNo { get; set; }
        public Guid? Hblid { get; set; }
        public bool? Negative { get; set; }
        public decimal? TotalPaidVnd { get; set; }
        public decimal? TotalPaidUsd { get; set; }
        public string CreditNo { get; set; }
        public decimal? CreditAmountVnd { get; set; }
        public decimal? CreditAmountUsd { get; set; }
        public decimal? ExchangeRateBilling { get; set; }
        public string PartnerId { get; set; }
        public bool? NetOff { get; set; }
        public decimal? NetOffVnd { get; set; }
        public decimal? NetOffUsd { get; set; }
    }
}
