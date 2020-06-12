using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.System.DL.Models.Criteria
{
    public class SysAuthorizedApprovalCriteria
    {
        public string All { get; set; }
        public string Authorizer { get; set; }
        public string Commissioner { get; set; }
        public string Type { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public bool? Active { get; set; }
    }
}
