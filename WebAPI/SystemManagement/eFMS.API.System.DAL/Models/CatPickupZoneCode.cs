using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class CatPickupZoneCode
    {
        public int Id { get; set; }
        public Guid PickupPlaceId { get; set; }
        public Guid BranchId { get; set; }
        public short ZoneId { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
    }
}
