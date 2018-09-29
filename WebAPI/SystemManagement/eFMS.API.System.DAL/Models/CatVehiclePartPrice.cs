using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class CatVehiclePartPrice
    {
        public int Id { get; set; }
        public Guid? WorkPlaceId { get; set; }
        public int VehiclePartId { get; set; }
        public decimal? Price { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
    }
}
