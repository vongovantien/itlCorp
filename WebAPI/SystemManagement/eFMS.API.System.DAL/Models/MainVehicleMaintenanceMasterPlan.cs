using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class MainVehicleMaintenanceMasterPlan
    {
        public Guid Id { get; set; }
        public Guid WorkPlaceId { get; set; }
        public string Code { get; set; }
        public string NamePlan { get; set; }
        public string Type { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
    }
}
