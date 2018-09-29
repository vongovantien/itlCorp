using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class CatDriverAppHistory
    {
        public int Id { get; set; }
        public int DriverId { get; set; }
        public string Imei { get; set; }
        public string Token { get; set; }
        public string Version { get; set; }
        public DateTime? StartedDate { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
    }
}
