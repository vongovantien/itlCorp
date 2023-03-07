using System;
using System.Collections.Generic;

namespace eFMS.API.Documentation.Service.Models
{
    public partial class SysTrackInfo
    {
        public Guid Id { get; set; }
        public DateTime? PlanDate { get; set; }
        public DateTime? ActualDate { get; set; }
        public string EventDescription { get; set; }
        public Guid? Station { get; set; }
        public string Status { get; set; }
        public string Quantity { get; set; }
        public string Weight { get; set; }
        public Guid? Hblid { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public string UserCreated { get; set; }
        public string FlightNo { get; set; }
        public string Type { get; set; }
        public string Unit { get; set; }
        public string Source { get; set; }
    }
}
