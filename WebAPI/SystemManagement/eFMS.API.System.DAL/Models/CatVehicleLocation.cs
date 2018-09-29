using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class CatVehicleLocation
    {
        public long Id { get; set; }
        public int VehicleId { get; set; }
        public string GeoCode { get; set; }
        public string Address { get; set; }
        public DateTime UpdatedTime { get; set; }
    }
}
