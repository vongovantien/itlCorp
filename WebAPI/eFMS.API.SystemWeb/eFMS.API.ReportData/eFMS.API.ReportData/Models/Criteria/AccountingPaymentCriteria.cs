using eFMS.API.ReportData.Models.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.ReportData.Models.Criteria
{
    public class AccountingPaymentCriteria
    {
        public string SearchType { get; set; }
        public List<string> ReferenceNos { get; set; }
        public string PartnerId { get; set; }
        public DateTime? IssuedDate { get; set; }
        public DateTime? FromIssuedDate { get; set; }
        public DateTime? FromUpdatedDate { get; set; }
        public DateTime? ToUpdatedDate { get; set; }
        public DateTime? DueDate { get; set; }
        public OverDueDate OverDueDays { get; set; }
        public List<string> PaymentStatus { get; set; }
        public PaymentType PaymentType { get; set; }
        public List<string> Office { get; set; }
        public bool? IsPaging { get; set; }
    }
}
