using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class CatPickupTime
    {
        public short Id { get; set; }
        public string FromTime { get; set; }
        public string ToTime { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
        public short? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }
}
