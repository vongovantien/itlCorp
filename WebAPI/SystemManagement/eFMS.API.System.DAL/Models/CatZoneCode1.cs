using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class CatZoneCode1
    {
        public short Id { get; set; }
        public string Code { get; set; }
        public string Type { get; set; }
        public decimal? DistanceFrom { get; set; }
        public decimal? DistanceTo { get; set; }
        public string Description { get; set; }
        public bool? IsSpecialZone { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
    }
}
