using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class PriceRouteCostShortTrip1
    {
        public Guid CostId { get; set; }
        public Guid PlaceFrom { get; set; }
        public Guid PlaceTo { get; set; }
        public decimal Kratio { get; set; }
        public int Length { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
    }
}
