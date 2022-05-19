using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Setting.DL.Models
{
    public class RuleLinkFeeImportModel
    {
        public Guid? Id { get; set; }
        public string ServiceBuying { get; set; }
        public string ServiceSelling { get; set; }
        public string ChargeBuying { get; set; }
        public string ChargeSelling { get; set; }
        public string PartnerBuying { get; set; }
        public string PartnerSelling { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public string RuleName { get; set; }
        public bool IsValid { get; set; }
        public string Status { get; set; }
    }
}
