using System;
using System.Collections.Generic;

namespace SystemManagementAPI.Service.Models
{
    public partial class MainMaintenanceQuotaDetail
    {
        public Guid Id { get; set; }
        public int MaintenanceQuotaId { get; set; }
        public short VehicleTypeId { get; set; }
        public decimal Quantity { get; set; }
        public string UserCreated { get; set; }
        public DateTime? DatetimeCreated { get; set; }
        public string UserModified { get; set; }
        public DateTime? DatetimeModified { get; set; }
        public bool? Inactive { get; set; }
        public DateTime? InactiveOn { get; set; }
    }
}
