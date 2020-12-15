using System;
using System.Collections.Generic;

namespace eFMS.API.Accounting.Service.Models
{
    public partial class AccAccountingPayment
    {
        public Guid Id { get; set; }
        public string BillingRefNo { get; set; }
        public Guid? ReceiptId { get; set; }
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
        public decimal? RefAmount { get; set; }
        public string RefCurrency { get; set; }
        public string Note { get; set; }
        public decimal? ReceiptExcPaidAmount { get; set; }
        public decimal? ReceiptExcBalance { get; set; }
        public decimal? ReceiptExcUnpaidAmount { get; set; }
    }
}
