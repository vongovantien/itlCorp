using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class OpsWawePickPlan
    {
        public OpsWawePickPlan()
        {
            OpsWawePickPlanItem = new HashSet<OpsWawePickPlanItem>();
        }

        public Guid Id { get; set; }
        public Guid WorkPlaceId { get; set; }
        public string SupplierId { get; set; }
        public string Code { get; set; }
        public DateTime? RequestedForDate { get; set; }
        public int? TotalQuantity { get; set; }
        public decimal? TotalVolume { get; set; }
        public decimal? TotalWeight { get; set; }
        public string Remark { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }

        public ICollection<OpsWawePickPlanItem> OpsWawePickPlanItem { get; set; }
    }
}
