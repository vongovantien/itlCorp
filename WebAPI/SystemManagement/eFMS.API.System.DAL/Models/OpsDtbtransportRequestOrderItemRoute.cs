using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class OpsDtbtransportRequestOrderItemRoute
    {
        public OpsDtbtransportRequestOrderItemRoute()
        {
            OpsWawePickPlanItem = new HashSet<OpsWawePickPlanItem>();
        }

        public Guid Id { get; set; }
        public Guid TransportRequestId { get; set; }
        public Guid OrderDropPointItemRouteId { get; set; }
        public int? Quantity { get; set; }
        public int? PickedUpActualQuantity { get; set; }
        public decimal? Weight { get; set; }
        public decimal? PickedUpActualWeight { get; set; }
        public decimal? PickedUpActualVolume { get; set; }
        public decimal? DeliveredActualWeight { get; set; }
        public int? DeliveredActualQuantity { get; set; }
        public decimal? DeliveredActualVolume { get; set; }
        public string Note { get; set; }
        public string CheckingInRemark { get; set; }
        public decimal? DropDownSurcharge { get; set; }
        public int? ShipmentStatusId { get; set; }
        public DateTime? PickupArrivedTime { get; set; }
        public DateTime? PickupLeftTime { get; set; }
        public DateTime? DeliveryArrivedTime { get; set; }
        public DateTime? DeliveryLeftTime { get; set; }
        public decimal? IndicatedCodvalue { get; set; }
        public decimal? Codvalue { get; set; }
        public decimal? Collection { get; set; }
        public bool? IsReturnWay { get; set; }
        public decimal? Rating { get; set; }
        public byte[] CustomerSignature { get; set; }
        public bool? IsArising { get; set; }
        public short? Sequence { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public string DeliveryStatus { get; set; }
        public string FailedDeliveryReason { get; set; }
        public string FailedDeliveryDueTo { get; set; }
        public DateTime? PodreceivedDate { get; set; }
        public DateTime? PodreturnedDate { get; set; }

        public OpsDtbtransportRequest TransportRequest { get; set; }
        public ICollection<OpsWawePickPlanItem> OpsWawePickPlanItem { get; set; }
    }
}
