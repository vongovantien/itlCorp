using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class CatReceptacleType
    {
        public CatReceptacleType()
        {
            CsReceptacleMaster = new HashSet<CsReceptacleMaster>();
        }

        public short Id { get; set; }
        public string Code { get; set; }
        public string UnitNameVn { get; set; }
        public string UnitNameEn { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }

        public ICollection<CsReceptacleMaster> CsReceptacleMaster { get; set; }
    }
}
