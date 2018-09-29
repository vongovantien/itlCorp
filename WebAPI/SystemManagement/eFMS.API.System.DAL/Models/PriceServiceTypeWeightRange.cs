using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class PriceServiceTypeWeightRange
    {
        public Guid Id { get; set; }
        public int? ServiceTypeMappingId { get; set; }
        public decimal Ladder { get; set; }
        public short UnitId { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
    }
}
