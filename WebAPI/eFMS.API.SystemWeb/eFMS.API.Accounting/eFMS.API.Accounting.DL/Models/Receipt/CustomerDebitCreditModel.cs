using System;
using System.Collections.Generic;

namespace eFMS.API.Accounting.DL.Models.Receipt
{
    public class CustomerDebitCreditModel
    {
        public string RefNo { get; set; }
        public string Type { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string PartnerId { get; set; }
        public string PartnerName { get; set; }
        public string TaxCode { get; set; }
        public string CurrencyId { get; set; }
        public decimal? Amount { get; set; }
        public decimal? PaidAmount { get; set; }
        public decimal? PaidAmountVnd { get; set; }
        public decimal? PaidAmountUsd { get; set; }
        public decimal? UnpaidAmount { get; set; }
        public decimal? UnpaidVnd { get; set; }
        public decimal? UnpaidUsd { get; set; }
        public decimal? PaymentTerm { get; set; }
        public DateTime? DueDate { get; set; }
        public string PaymentStatus { get; set; }
        public int? DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public Guid? OfficeId { get; set; }
        public string OfficeName { get; set; }
        public Guid? CompanyId { get; set; }
        public List<string> RefIds { get; set; }
        public string CreditType { get; set; }
    }
}
