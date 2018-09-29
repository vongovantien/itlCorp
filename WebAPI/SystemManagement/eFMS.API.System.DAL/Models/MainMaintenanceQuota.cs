using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class MainMaintenanceQuota
    {
        public int Id { get; set; }
        public Guid WorkPlaceId { get; set; }
        public int VehiclePartId { get; set; }
        public int Value { get; set; }
        public short? UnitId { get; set; }
        public decimal? Deviation { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
    }
}
