using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class CatCountry
    {
        public CatCountry()
        {
            CatHub = new HashSet<CatHub>();
            CatProvince = new HashSet<CatProvince>();
            SysBu = new HashSet<SysBu>();
        }

        public short Id { get; set; }
        public string Code { get; set; }
        public string NameVn { get; set; }
        public string NameEn { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }

        public ICollection<CatHub> CatHub { get; set; }
        public ICollection<CatProvince> CatProvince { get; set; }
        public ICollection<SysBu> SysBu { get; set; }
    }
}
