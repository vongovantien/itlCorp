using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class MainReplacedVehiclePartStatus
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public int VehiclePartId { get; set; }
        public decimal? ReplacedQuatity { get; set; }
        public DateTime? ReplacedDate { get; set; }
        public int? ContermetNumberInReplacedTime { get; set; }
        public int? TotalLength { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
    }
}
