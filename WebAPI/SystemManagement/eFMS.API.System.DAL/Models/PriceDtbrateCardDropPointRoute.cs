using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class PriceDtbrateCardDropPointRoute
    {
        public Guid Id { get; set; }
        public Guid PickupPointId { get; set; }
        public Guid DeliveryPointId { get; set; }
        public Guid RateCardDetailId { get; set; }
    }
}
