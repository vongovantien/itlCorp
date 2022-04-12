using System;
using System.Collections.Generic;

namespace eFMS.API.Accounting.DL.Models.Criteria
{
    public class AcctCombineBillingCriteria
    {
        public List<string> ReferenceNo { get; set; }
        public string PartnerId { get; set; }
        public List<string> Creator { get; set; }
        public DateTime? CreatedDateFrom { get; set; }
        public DateTime? CreatedDateTo { get; set; }
        public string Currency { get; set; }
    }
}
