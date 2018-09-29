using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class CatVehiclePartDetailHistory
    {
        public int Id { get; set; }
        public int VehicleWorkPlaceId { get; set; }
        public Guid? WorkPlaceId { get; set; }
        public int? VehicleId { get; set; }
        public int VehiclePartId { get; set; }
        public string Serial { get; set; }
        public string Description { get; set; }
        public int? LengthInReplacement { get; set; }
        public DateTime? ReplacedDate { get; set; }
        public bool? Liquidated { get; set; }
        public DateTime? LiquidatedDate { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
    }
}
