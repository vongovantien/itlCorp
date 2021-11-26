using System;
using System.Collections.Generic;

namespace eFMS.API.Setting.Service.Models
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
        public DateTime? ExpirationDate { get; set; }
        public string NameRule { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Active { get; set; }
        public DateTime? InactiveOn { get; set; }
    }
}
