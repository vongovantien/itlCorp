using System;

namespace eFMS.API.Accounting.DL.Models.AccountingPayment
{
    public class AccountingPaymentModel
    {
        public string RefId { get; set; }
        public string RefNo { get; set; }
        public string InvoiceNoReal { get; set; }
        public string SOANo { get; set; }
        public string PartnerId { get; set; }
        public string PartnerName { get; set; }
        public decimal? Amount { get; set; }
        public string Currency { get; set; }
        public DateTime? IssuedDate { get; set; }
        public string Serie { get; set; }
        public decimal? UnpaidAmount { get; set; }
        public decimal? PaidAmount { get; set; }
        public DateTime? DueDate { get; set; }
        public int OverdueDays { get; set; }
        public string Status { get; set; }
        public int? ExtendDays { get; set; }
        public string ExtendNote { get; set; }
        public string Type { get; set; }
        public string VoucherId { get; set; }
        public Guid? ReceiptId { get; set; }
        public DateTime? ConfirmBillingDate { get; set; }
        public Guid? OfficeId { get; set; }
        public string ServiceType { get; set; }
        public DateTime? PaidDate { get; set; }
        public decimal? UnpaidAmountVnd { get; set; }
        public decimal? TotalAmount { get; set; }
    }
}
