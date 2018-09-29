using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class PriceBuyingRoute
    {
        public Guid Id { get; set; }
        public Guid BuyingId { get; set; }
        public Guid PlaceFrom { get; set; }
        public Guid PlaceTo { get; set; }
        public string RoadId { get; set; }
        public int ServiceTypeMappingId { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
    }
}
