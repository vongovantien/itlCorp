using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class SysChangeBookingOverDateLog
    {
        public int Id { get; set; }
        public string CustomerId { get; set; }
        public int OldValue { get; set; }
        public int NewValue { get; set; }
        public DateTime? ReChangedDate { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? ReChanged { get; set; }
        public bool? Unlimited { get; set; }
    }
}
