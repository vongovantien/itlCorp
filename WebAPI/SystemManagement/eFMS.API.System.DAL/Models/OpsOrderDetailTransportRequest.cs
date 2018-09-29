using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class OpsOrderDetailTransportRequest
    {
        public long Id { get; set; }
        public Guid TransportRequestId { get; set; }
        public Guid OrderDetailId { get; set; }
        public decimal? Weight { get; set; }
        public decimal? ActualWeight { get; set; }
        public decimal? ActualVolume { get; set; }
        public string Note { get; set; }
        public string CheckingInRemark { get; set; }
        public bool? FreeDropCharge { get; set; }
        public decimal? DropDownSurcharge { get; set; }
        public int? ShipmentStatusId { get; set; }
        public bool? IsReturnWay { get; set; }
        public decimal? IndicatedCodvalue { get; set; }
        public decimal? Codvalue { get; set; }
        public decimal? Collection { get; set; }
        public decimal? Rating { get; set; }
        public byte[] CustomerSignature { get; set; }
        public bool? IsArising { get; set; }
        public bool? IsHandedOver { get; set; }
        public int? Sequence { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public string DeliveryStatus { get; set; }
        public string FailedDeliveryReason { get; set; }
        public string FailedDeliveryDueTo { get; set; }
        public DateTime? PodreceivedDate { get; set; }
        public DateTime? PodreturnedDate { get; set; }
    }
}
