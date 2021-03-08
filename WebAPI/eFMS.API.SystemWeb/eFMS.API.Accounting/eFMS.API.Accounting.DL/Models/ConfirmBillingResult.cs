using System;

namespace eFMS.API.Accounting.DL.Models
{
    public class ConfirmBillingResult
    {
        public Guid Id { get; set; }
        public string InvoiceNoReal { get; set; }
        public string PartnerId { get; set; }
        public string PartnerName { get; set; }
        public decimal? TotalAmount { get; set; }
        public string Currency { get; set; }
        public string Type { get; set; }
        public DateTime? Date { get; set; }
        public decimal? PaymentTerm { get; set; }
        public DateTime? ConfirmBillingDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string PaymentStatus { get; set; }
        public DateTime? DatetimeModified { get; set; }
    }
}
