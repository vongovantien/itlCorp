using System;
using System.Collections.Generic;
using System.Text;

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
        public decimal? Amount { get; set; }
        public decimal? UnpaidAmount { get; set; }
        public decimal? UnpaidVnd { get; set; }
        public decimal? UnpaidUsd { get; set; }
        public decimal? PaymentTerm { get; set; }
        public DateTime? DueDate { get; set; }
        public string PaymentStatus { get; set; }
        public Guid DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public Guid OfficeId { get; set; }
        public string OfficeName { get; set; }
    }
}
