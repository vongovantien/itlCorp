using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class MainVehicleMaintenancePlace
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public Guid? WorkPlaceId { get; set; }
        public string NameVn { get; set; }
        public string Address { get; set; }
        public string ContactName { get; set; }
        public string Tel { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
    }
}
