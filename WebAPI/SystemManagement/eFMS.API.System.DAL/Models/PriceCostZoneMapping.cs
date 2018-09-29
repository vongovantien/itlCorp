using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class PriceCostZoneMapping
    {
        public Guid Id { get; set; }
        public Guid CostId { get; set; }
        public short PickupZoneId { get; set; }
        public short DeliveryZoneId { get; set; }
        public decimal? UnitCost { get; set; }
    }
}
