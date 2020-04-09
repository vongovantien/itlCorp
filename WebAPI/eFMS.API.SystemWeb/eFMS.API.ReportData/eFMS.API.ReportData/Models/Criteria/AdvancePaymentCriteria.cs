using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.ReportData.Models.Criteria
{
    public class AdvancePaymentCriteria
    {
        public List<string> ReferenceNos { get; set; }
        public string Requester { get; set; }
        public DateTime? RequestDateFrom { get; set; }
        public DateTime? RequestDateTo { get; set; }
        public DateTime? AdvanceModifiedDateFrom { get; set; }
        public DateTime? AdvanceModifiedDateTo { get; set; }
        public string PaymentMethod { get; set; }
        public string StatusApproval { get; set; }
        public string StatusPayment { get; set; }
        public string CurrencyID { get; set; }
    }
}
