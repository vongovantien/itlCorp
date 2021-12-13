using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Setting.DL.Models.Criteria
{
    public class CsRuleLinkFeeCriteria
    {
        public string RuleName { get; set; }
        public string Datetype { get; set; }
        public string ServiceBuying { get; set; }
        public string ServiceSelling { get; set; }
        public string PartnerBuying { get; set; }
        public string PartnerSelling { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool? Status { get; set; }
    }
}
