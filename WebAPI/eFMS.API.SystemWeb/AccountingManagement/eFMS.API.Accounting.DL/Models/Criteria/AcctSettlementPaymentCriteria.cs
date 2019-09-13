using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Accounting.DL.Models.Criteria
{
    public class AcctSettlementPaymentCriteria
    {
        public List<string> ReferenceNos { get; set; }
        public string Requester { get; set; }
        public DateTime? RequestDateFrom { get; set; }
        public DateTime? RequestDateTo { get; set; }
        public DateTime? AdvanceDateFrom { get; set; }
        public DateTime? AdvanceDateTo { get; set; }
        public string PaymentMethod { get; set; }
        public string StatusApproval { get; set; }
    }
}
