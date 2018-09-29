using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class MainVehicleRepairLevel
    {
        public int Id { get; set; }
        public int RepairLevel { get; set; }
        public decimal? Price { get; set; }
        public string Note { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool Inactive { get; set; }
        public DateTime InactiveOn { get; set; }
    }
}
