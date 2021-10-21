using System;
using System.Collections.Generic;

namespace eFMS.API.ReportData.Models.Criteria
{
    public class SettlementPaymentCriteria
    {
        public List<string> ReferenceNos { get; set; }
        public string Requester { get; set; }
        public DateTime? RequestDateFrom { get; set; }
        public DateTime? RequestDateTo { get; set; }
        public DateTime? AdvanceDateFrom { get; set; }
        public DateTime? AdvanceDateTo { get; set; }
        public string PaymentMethod { get; set; }
        public string StatusApproval { get; set; }
        public string CurrencyID { get; set; }
        public string PayeeId { get; set; }
        public int? DepartmentId { get; set; }
    }
}
