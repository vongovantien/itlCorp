using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class PriceServiceTypeWeightRangeDetail
    {
        public Guid ServiceTypeWeightRangeId { get; set; }
        public decimal FromValue { get; set; }
        public decimal ToValue { get; set; }
        public short UnitId { get; set; }
        public bool? IsMinCharges { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
    }
}
