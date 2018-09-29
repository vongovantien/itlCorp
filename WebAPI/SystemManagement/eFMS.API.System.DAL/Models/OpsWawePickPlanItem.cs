using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class OpsWawePickPlanItem
    {
        public Guid Id { get; set; }
        public Guid WawePickPlanId { get; set; }
        public Guid TransportRequestItemId { get; set; }
        public string TransportRequestItemType { get; set; }
        public Guid PlaceId { get; set; }
        public string Remark { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }

        public OpsDtbtransportRequestOrderItemRoute TransportRequestItem { get; set; }
        public OpsWawePickPlan WawePickPlan { get; set; }
    }
}
