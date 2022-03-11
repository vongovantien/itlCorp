using System;
using System.Collections.Generic;

namespace eFMS.API.Documentation.Service.Models
{
    public partial class CsLinkCharge
    {
        public Guid Id { get; set; }
        public string LinkChargeType { get; set; }
        public string JobNoOrg { get; set; }
        public string JobNoLink { get; set; }
        public string ChargeOrgId { get; set; }
        public string ChargeLinkId { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public string UserModified { get; set; }
    }
}
