using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class CatVehiclePartDetail
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public int VehiclePartId { get; set; }
        public int Quantity { get; set; }
        public string Description { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
    }
}
