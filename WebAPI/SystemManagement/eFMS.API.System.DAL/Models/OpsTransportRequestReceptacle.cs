using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class OpsTransportRequestReceptacle
    {
        public long Id { get; set; }
        public Guid? TransportRequestId { get; set; }
        public int? ReceptacleMasterId { get; set; }
        public string Note { get; set; }
        public int? CheckingInOrderReasonId { get; set; }
        public string CheckingInRemark { get; set; }
        public bool? IsReturnWay { get; set; }
        public bool? IsHandedOver { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
    }
}
