using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class CatPartnerGroup
    {
        public CatPartnerGroup()
        {
            CatPartner = new HashSet<CatPartner>();
        }

        public string Id { get; set; }
        public string GroupNameVn { get; set; }
        public string GroupNameEn { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }

        public ICollection<CatPartner> CatPartner { get; set; }
    }
}
