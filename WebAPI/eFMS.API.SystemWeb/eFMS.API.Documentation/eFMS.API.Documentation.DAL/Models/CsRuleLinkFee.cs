using System;
using System.Collections.Generic;

namespace eFMS.API.Documentation.Service.Models
{
    public partial class CsRuleLinkFee
    {
        public Guid Id { get; set; }
        public string ServiceBuying { get; set; }
        public string ServiceSelling { get; set; }
        public string ChargeBuying { get; set; }
        public string ChargeSelling { get; set; }
        public string PartnerBuying { get; set; }
        public string PartnerSelling { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public string RuleName { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Status { get; set; }
    }
}
