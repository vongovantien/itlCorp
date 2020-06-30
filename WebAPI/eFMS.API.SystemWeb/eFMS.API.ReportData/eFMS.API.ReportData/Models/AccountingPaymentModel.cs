using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.ReportData.Models
{
    public class AccountingPaymentModel
    {
        public string RefId { get; set; }
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
    }
}
