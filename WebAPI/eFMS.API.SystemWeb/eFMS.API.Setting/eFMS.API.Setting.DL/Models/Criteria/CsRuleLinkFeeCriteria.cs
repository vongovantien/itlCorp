using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Setting.DL.Models.Criteria
{
    public class CsRuleLinkFeeCriteria
    {
        public string ServiceBuying { get; set; }
        public string ServiceSelling { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public bool? Status { get; set; }
    }
}
