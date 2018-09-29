using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class CsDtborderDropPointItemRoute
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid PickupPointItemId { get; set; }
        public Guid DeliveryPointId { get; set; }
        public int Quantity { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Volume { get; set; }
        public DateTime? PickupRequestDate { get; set; }
        public string PickupRequestFromTime { get; set; }
        public string PickupRequestToTime { get; set; }
        public DateTime? PickupArrivedTime { get; set; }
        public DateTime? PickupLeftTime { get; set; }
        public DateTime? DeliveryRequestDate { get; set; }
        public string DeliveryRequestFromTime { get; set; }
        public string DeliveryRequestToTime { get; set; }
        public DateTime? DeliveryArrivedTime { get; set; }
        public DateTime? DeliveryLeftTime { get; set; }
        public string Note { get; set; }
        public int? AssignedQuantity { get; set; }
        public decimal? AssignedTransportWeight { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public int? CurrentStatusId { get; set; }

        public CsDtborder Order { get; set; }
    }
}
