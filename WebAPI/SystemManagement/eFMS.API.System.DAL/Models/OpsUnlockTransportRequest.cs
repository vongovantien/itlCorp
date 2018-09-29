using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class OpsUnlockTransportRequest
    {
        public Guid Id { get; set; }
        public Guid TransportId { get; set; }
        public string ShipmentType { get; set; }
        public string OpsmanId { get; set; }
        public string OpsmanStatus { get; set; }
        public DateTime? OpsmanDate { get; set; }
        public string OpsmanNote { get; set; }
        public string ChiefId { get; set; }
        public string ChiefStatus { get; set; }
        public string ChiefNote { get; set; }
        public DateTime? ChiefDate { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UnlockReason { get; set; }
        public Guid? WorkPlaceId { get; set; }
        public string UnlockType { get; set; }
    }
}
