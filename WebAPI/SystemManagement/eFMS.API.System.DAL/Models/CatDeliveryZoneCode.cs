using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class CatDeliveryZoneCode
    {
        public int Id { get; set; }
        public Guid OriginBranchId { get; set; }
        public Guid ToPlace { get; set; }
        public short ZoneId { get; set; }
        public bool? IsRas { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
    }
}
